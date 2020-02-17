Verto est une collection d’assemblées qui implémente CELCAT Data Store (CTDS), un référentiel de données qui donne aux utilisateurs finaux un accès aux données d’emplois du temps consolidées de CT7 (CELCAT v7) et CT8 (CELCAT v8), mises à jour périodiquement à partir des bases de données d’emplois du temps sources.
Le fonctionnement de CTDS est décrit ci-dessous:

1.	Les bases de données d’emplois du temps sources sont copiées vers une zone intermédiaire dans une base de données ADMIN. Ce processus est accompli rapidement en utilisant Sql Bulk Insert et son impact sur la performance des bases de données sources est faible. Veuillez noter qu’il y a en réalité trois schémas intermédiaires dans la base de données ADMIN: “Primaire”, “Secondaire” et “Temp”. Les emplois du temps sources sont toujours copiés tout d’abord vers le schéma Temp.

2.	Quand les emplois du temps ont été copiés avec succès vers le schéma Temp, une vérification de validité prend place. Si tout va bien, le schéma Primaire est copié vers le schéma Secondaire et le schéma Temp est copié vers le schéma Primaire. Toutefois, si des problèmes d’intégrité se présentent, le processus est abandonné – le tout reste cependant dans un état cohérent.

3.	Un Diff prend place dans les schémas Primaire et Secondaire pour analyser les changements ayant eu lieu dans la dernière extraction. Les changements sont enregistrés dans le schéma “History“ de la base de données ADMIN.

4.	Toute ressource nouvellement ajoutée se voit assigner un "Federation ID" dans le schéma "Federation" de la base de données ADMIN. C’est là que les ressources provenant de plusieurs emplois du temps reçoivent un Id unique qui sera utilisé dans le CTDS public.

5.	Des "Consolidation Id" sont aussi générés afin de lier les ressources (provenant de différents emplois du temps) qui doivent être traitées comme étant la même ressource physique. Cette information est stockée dans le schéma Federation et on appelle la collection de tables "consolidation map". Voir le fichier DataStoreConfig.xml, l’élément "consolidation" pour savoir sur quelle base les ressources sont consolidées. 

6.	Le schéma History est mis à jour pour que tous les id soient bien des id fédérés plutôt que ceux des emplois du temps natifs. 

7.	Les lignes nouvellement générées dans le schéma History sont alors copiées (en utilisant Bulk Insert) vers le schéma intermédiaire de la base de données  PUBLIC, comme l’est le “consolidation map”.
