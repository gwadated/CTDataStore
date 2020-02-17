# Data Store Design

CT Data Store (CTDS) est une collection d’assemblées .NET contenant des fonctions permettant de générer et de maintenir un référentiel de données qui vous donne accès aux données d’emplois du temps CT7 consolidées, mise à jour périodiquement à partir des bases de données d’emplois du temps sources. Le référentiel est normalement utilisé à des fins de reporting et de publication. Les applications clientes devraient le considérer en lecture seule.

![IMG.1.0](images/datastore1.0.fr.png?raw=true)

CTDS est disponible sous une forme que vous pourrez utiliser avec vos propres applications .NET. Autrement, une application de console autonome est également fournie et peut être utilisée pour ‘conduire’ le processus (soit manuellement, soit en utilisant le Planificateur de tâches de Windows).

# Vue d’ensemble du flux de données

CTDS assure le déplacement et la transformation des données. Les différentes phases sont :

1.	Extraction des emplois du temps
2.	Vérification d’intégrité dans la phase intermédiaire
3.	Extraction Delta
4.	Fédération et consolidation
5.	Phase intermédiaire publique
6.	Transformation et alimentation

Le flux de données est décrit ci-dessous :

## Extraction des emplois du temps

Les bases de données d’emplois du temps sources spécifiées sont copiées (presque intégralement) vers une zone intermédiaire dans une base de données ADMIN. La base de données ADMIN et les bases de données d’emplois du temps sources n’ont pas nécessairement besoin d’être sur le même serveur. La copie est effectuée rapidement en utilisant Sql Bulk Insert et a peu d’impact sur la performance et la disponibilité des bases de données sources (ce qui est capital).

> **Remarque:** On trouve trois schémas intermédiaires dans la base de données ADMIN :  “Primaire”, “Secondaire” et “Temp” (les schémas peuvent être considérés comme des moyens d’organiser les tables de données dans une base de données, permettant de regrouper les tables associées). Les emplois du temps sources sont toujours d’abord copiés vers le schéma Temp. Le rôle de ces trois schémas est souligné plus loin dans ce document.

Un élément important est ajouté aux tables intermédiaires: la colonne appelée “src_timetable_id”, qui est un identificateur unique de l’emploi du temps source. Il permet à CTDS d’extraire les données de plusieurs bases de données d’emplois du temps tout en étant en mesure de déterminer l’origine de chaque ligne. Par exemple, l’illustration ci-dessous montre le contenu de la table CT_DEPT dans la zone intermédiaire ADMIN – il s’agit des enregistrements de départements provenant de deux emplois du temps sources: 

![IMG.1.1](images/datastore1.1.png?raw=true)

Les données dans les tables intermédiaires sont presque identiques à celles des tables sources, à part le fait que la colonne src_timetable_id a été ajoutée. Elle est générée par CTDS et fait référence à une ligne dans la table CONTROL.SOURCE_TIMETABLE table.

> **Remarque:** Les tables intermédiaires n’ont pas d’index, de clés primaires ni de contraintes d’intégrité référentielle ; elles sont conçues pour permettre s’insérer rapidement les données des bases de données sources.

Les données d’Attendance (suivi de l’assiduité) sont légèrement modifiées durant l’étape intermédiaire dans le but de simplifier leur structure. Les tables CT_AT_EXCEPTION, CT_AT_STUDENT_EXCEPTION, CT_AT_STUDENT_REGISTER et CT_AT_STUDENT_MARK n’ont pas de phase intermédiaire – leur contenu est combiné et stocké dans une seule nouvelle table dans le schéma intermédiaire, appelée CT_AT_REGISTER_MARK.

## Vérification d’intégrité dans la phase intermédiaire

Quand les emplois du temps sources ont été copiés vers le schéma intermédiaire, une vérification d’intégrité prend place. Dans la phase Extraction, les données sont tirées des emplois du temps sources, table après table – il est donc possible qu’une référence de données dans une des tables intermédiaires ne soit pas valide dans la table primaire. Par exemple, supposons que CTDS extraie des données d’une table CT_ROOM et certaines des salles appartiennent au département dont dept_id = 10. Quelques secondes plus tard, CTDS extrait les lignes de la table CT_DEPT mais entretemps, ce département a été supprimé. Certaines salles dans la base de données intermédiaire se retrouveront donc avec des départements non valides. On appelle ces cas des « références rompues » dans la routine de vérification d’intégrité de CTDS.

Une autre vérification est effectuée pour veiller à ce que toutes les valeurs de « clés de consolidation» (« consolidation key ») soient non-null. La consolidation désigne le processus de mise en correspondance d’enregistrements similaires sur plusieurs bases de données. Par exemple, si vous spécifiez que le personnel doit être mis en correspondance sur les différents emplois du temps en utilisant leurs adresses emails, CTDS va vérifier que chaque membre du personnel a bien une adresse email non-null.

A ce stade, il est bien possible que les données intermédiaires échouent lors de la vérification d’intégrité. Dans ce cas, le processus est abandonné et un message d’erreur apparait. Vous aurez l’option d’effectuer une nouvelle tentative plus tard. Heureusement, parce que les données intermédiaires se trouvent dans le schéma intermédiaire Temp, le processus abandonné n’affectera pas le référentiel de données ; ce dernier conserve un état cohérent.

Si tout va bien durant la phase intermédiaire, les données intermédiaires Temp sont promues au schéma primaire (après que ce dernier ait d’abord été archivé dans le schéma secondaire). Ce déplacement de données (illustré ci-dessous) est performant car il consiste tout simplement à renommer le schéma. Il s’agit d’une étape importante au stade suivant dans le flux de travail de CTDS – découvrir ce qui a changé.

![IMG.1.2](images/datastore1.2.fr.png?raw=true)

## Extraction Delta

A ce stade du flux de données, le schéma intermédiaire primaire contient l’extrait d’emplois du temps le plus récent et le schéma secondaire contient l’extrait qui le précède. CTDS va maintenant réaliser un “diﬀ” sur les deux schémas pour trouver le delta (c.-à-d. ce qui a changé dans les emplois du temps). Tout changement détecté sera enregistré dans le schéma “History” de la base de données ADMIN quand il y a une table correspondante pour chacune des tables intermédiaires. On trouve des colonnes de plus dans le schéma History, y compris les trois colonnes ci-dessous :

![IMG.1.3](images/datastore1.3.png?raw=true)

La colonne “history_status” est utilisée pour indiquer si les changements de données sont un Insert (“I”), un Update (“U”) (mise à jour) ou un Deletion “D” (suppresion).

La colonne “history_stamp” enregistre à quel moment la ligne a été ajoutée à l’historique. 

“history_control_log_id is » fait référence à la table CONTROL.LOG qui conduit une piste d’audit de l’activité de CTDS et peut servir à résoudre des problèmes, etc. Veuillez noter le contenu de la table LOG ci-dessous avec l’id = 5 mis en surbrillance – cette entrée correspond aux lignes de la table d’historique ci-dessus. 

![IMG.1.4](images/datastore1.4.png?raw=true)

Le schéma History n’est jamais purgé ; il conserve tous les deltas (données changées) insérés. Un tel historique des changements maintenu est une source de données utile par ailleurs. Extraire les deltas permet aussi à CTDS d’alimenter petit à petit et au fur et à mesure les changements dans la banque de données utilisée par les utilisateurs plutôt que mettre à jour toute la banque de données en une fois, comme illustré ci-dessous :

![IMG.1.5](images/datastore1.5.fr.png?raw=true)

Alimenter les données de la banque de données publique de cette manière fera en sorte qu’il ne soit pas soumis à des changements à grande échelle qui affecteront sa disponibilité. Les changements se produisent de façon incrémentée et au fur et à mesure que des changements sont identifiés dans les emplois du temps sources.

## Fédération et consolidation

La table History contient d’autres colonnes importantes, comme illustré dans la vue suivante de la table Department du schéma History. Les colonnes en surbrillance contiennent des valeurs d’id “fédérées” et “consolidées” pour les différentes colonnes clés dans la table Department.

![IMG.1.6](images/datastore1.6.png?raw=true)

Un **Federated Id** est une valeur bigint générée par CTDS et identifie de manière unique une ressource dans la banque de données. Par exemple, supposons que Département A dans la base de données d’emploi du temps X a une valeur dept_id de 100 dans cette base de données, et que Département B dans la base de données d’emploi du temps Y a aussi un id de 100. Quand CTDS extrait les données de ces deux emplois du temps et les combine dans une seule banque de données, il génèrera un identificateur unique pour chaque enregistrement de département (par exemple, 90123 pour Département A et 90124 pour Département B). Il s’agit de l’id fédérée (federated id) qui est utilisée dans le schéma History en se référant à une table primaire dans le schéma “Federation” (appelé FEDERATION.DEPT pour les départements), qui contient les informations de l’emploi du temps source de chaque ressource. 

**Point clé** – Chaque entité d’emploi du temps (personnel, département, événement, etc) reçoit une nouvelle identité quand elle est tirée vers la banque de données. Cette identité est appelée “Federated Id” et sert à l’identifier de manière unique parmi les autres entités du même type qui pourraient avoir les même valeurs d’id natives. Le schéma FEDERATION fournit un mappage entre les valeurs d’id natives et fédérées.

Une fois que le delta a été enregistré dans le schéma History (voir les étapes précédentes), CTDS renseigne des colonnes supplémentaires d’id “fédérées” et “consolidées”. Ces actions sont réalisées dans cet ordre strict : 

1.	Tout élément de History ayant le statut Deleted (supprime) (history_status = 'D') est mis à jour en premier en utilisant les valeurs d’id de fédération qui existent déjà dans le schéma Federation (puisque les éléments doivent d’abord avoir été insérés précédemment).
 
2.	De nouveaux id de fédération sont générés pour tous les autres éléments de History (Inserts et Updates) et les valeurs sont appliquées aux tables History. 

3.	Les tables Federation sont purgées des données correspondant aux lignes supprimées. Veuillez noter que les valeurs d’id de fédération ne sont pas réutilisées.  

4.	Au fur et à mesure que les lignes de History sont mises à jour avec les valeurs d’id de fédération, CTDS place un indicateur “history_federated” sur la ligne pour indiquer que la fédération de la ligne est terminée.  

La consolidation est un processus similaire à la fédération et se produit en même temps. La consolidation prend place pour un sous-ensemble de types d’entités (qui comprend les principaux types de ressources) et permet à plusieurs entités de différents emplois du temps (voire du même emploi du temps) d’être liées ensemble et traitées comme étant la même ressource physique. Cette mise en lien d’entités est effectuée automatiquement par CTDS en se basant sur les paramètres de consolidation spécifiés dans un fichier xml ou par programme. En fait, il s’agit simplement de spécifier quelles colonnes doivent être utilisées quand on essaie de mettre en correspondance des éléments d’un type particulier, par exemple utiliser les adresses emails quand on met en correspondance les enregistrements du personnel, ou le champ unique_name (code) quand on met en correspondance les départements, etc.

Une fois de plus, un identificateur est généré (appelé “Consolidated Id”), mais cette fois-ci il peut faire référence à plus d’une ressource. Ces informations sont stockées dans le schéma Federation et il est possible de mapper les id consolidés, les id fédérés et les id des entités dans les emplois du temps d’origine. 

## Phase intermédiaire dans PUBLIC

Jusqu’à présent, toutes les mises à jour des bases de données ont eu lieu dans la base de données ADMIN, qui n’est généralement disponibles qu’aux administrateurs système ou à ceux qui travaillent dans le reporting. Une fois que le schéma History a été mis à jour, CTDS génère un nouveau schéma intermédiaire dans la base de données PUBLIC.

La première étape de cette phase consiste à veiller à ce qu’il n’y ait pas de transformations devant encore être réalisées dans le schéma intermédiaire public. S’il y en a, la phase sera abandonnée et un message d’erreur conséquent apparaitra.

La zone intermédiaire Public contient les changements dans les données d’emplois du temps (le delta). Une fois que la phase intermédiaire est terminée, CTDS marque les lignes correspondantes dans le schéma History comme étant “appliquées” en y plaçant l’indicateur appelé “history_applied”. Il indique qu’une ligne particulière dans le schéma History a été transférée à la banque de données publique (ou au moins à la zone intermédiaire Public) ; cette ligne est de fait ôtée de sorte qu’elle ne soit pas de nouveau traitée. 

## Transformation et alimentation

La phase de transformation déplace les données de la zone intermédiaire Public vers la banque de données publique. La banque de données publique comprend plusieurs schémas, tels que Resource, Attendance, Event, etc – des groupements logiques de tables associées. 

Durant la transformation, les données sur les événements sont développées par semaine. Donc, si un événement originel a lieu sur 10 semaines par exemple, CTDS va générer 10 ‘instances d’événements’ dans la banque de données publique. Le format originel (avec la chaine de semaine YN) est aussi conservé, ce qui veut dire que vous pouvez interroger la base de données publique soit par événements CELCAT standard, soit par instances d’événements.

Enfin, la zone intermédiaire Public est tronquée.

## Mutex

Un mutex de base de données est créé lors du traitement par CTDS en tenant compte des paramètres dans la table CONFIG du schéma Control. Il est conçu pour empêcher que plusieurs processus CTDS opèrent sur les données en même temps.
