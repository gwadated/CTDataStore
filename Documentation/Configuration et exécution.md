# Configuration et exécution

Le programme CTDataStore.exe est utilisé pour assurer la maintenance de la banque de données. Spécifiez votre configuration dans le fichier **DataStoreConfig.xml** puis exécutez CTDataStore.exe pour générer la banque de données. Initialement, créer la banque de données prendra un peu de temps, mais les opérations suivantes devraient être bien plus rapides (en particulier si peu de changements ont eu lieu dans les emplois du temps sources).

Vous trouverez ci-dessous un exemple de fichier DataStoreConfig.xml ainsi qu’une description des différentes sections: 


``` xml
<?xml version="1.0" encoding="utf-8"?>
<dataStoreConfiguration
xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
xmlns:xsd="http://www.w3.org/2001/XMLSchema">
    <source>
        <timetables>
            <timetable>
                <connectionString>Server=PC8\SQLSVR2012; Database=MERCIA_2014_15; Trusted_Connection=True</connectionString>
            </timetable>
            <timetable>
                <connectionString>Server=PC8\SQLSVR2012; Database=MERCIA_2015_16; Trusted_Connection=True</connectionString>
            </timetable>
        </timetables>
    </source>
    <destination>
        <adminDatabase>
            <connectionString>Server=PC8\SQLSVR2012; Database=CTDS_ADMIN; Trusted_Connection=True</connectionString>
        </adminDatabase>
        <publicDatabase>
            <connectionString>Server=PC8\SQLSVR2012; Database=CTDS_PUBLIC; Trusted_Connection=True</connectionString>
        </publicDatabase>
    </destination>
    <commandTimeouts sourceTimetables="60" adminDatabase="240" publicDatabase="480" />
    <forceRebuild>false</forceRebuild>
    <maxDegreeOfParallelism>4</maxDegreeOfParallelism>
    <pipelines>
        <adminStaging singleThreaded="false" />
        <adminHistory singleThreaded="false" />
        <adminDiff singleThreaded="false" />
        <publicConsolidation singleThreaded="false" />
        <publicStaging singleThreaded="false" />
        <publicTransformation singleThreaded="false" />
        <publicTempUpsert singleThreaded="false" />
    </pipelines>
    <consolidation enabled="true">
        <entry entity="room" column="unique_name" />
        <entry entity="staff" column="unique_name" />
        <entry entity="course" column="name" />
        <entry entity="module" column="unique_name" />
        <entry entity="group" column="unique_name" />
        <entry entity="student" column="unique_name" />
        <entry entity="equip" column="unique_name" />
        <entry entity="team" column="unique_name" />
        <entry entity="faculty" column="name" />
        <entry entity="dept" column="name" />
        <entry entity="fixture" column="name" />
        <entry entity="layout" column="name" />
        <entry entity="site" column="name" />
        <entry entity="eventCat" column="name" />
        <entry entity="supervisor" column="name" />
        <entry entity="staffCat" column="name" />
        <entry entity="user" column="name" />
    </consolidation>
</dataStoreConfiguration>
```

## Source

La section Source spécifie les bases de données d’emplois du temps sources. Les connexions aux bases de données de chaque emploi du temps dans la section des emplois du temps sont configurés à l’aide de [chaînes de connexion](https://www.connectionstrings.com/sql-server/) standard. 

## Destination

La section Destination concerne l’emplacement de la banque de données – les bases de données ADMIN et PUBLIC. Des chaînes de connexions standard sont aussi utilisées ici. Vous pouvez placer les bases de données source, admin et public sur des serveurs différents si nécessaire.

## Consolidation

La section Consolidation décrit la façon dont les différents types d’entités sont mis en correspondance par le processus CTDS. Par exemple, l’entrée suivante indique que les salles devront être mises en correspondance en utilisant le code (« unique name »):

``` xml
<entry entity="room" column="unique_name" />
```

Si cette configuration est spécifiée, CTDS va automatiquement consolider les enregistrements des salles sur les différents emplois du temps et combiner tous les enregistrements de salle dont le code est “A1”, par exemple. Si un type d’entité en particulier n’a pas d’entrée dans cette section, les enregistrements de ce type ne seront pas consolidés. Vous trouverez ci-dessous une liste de types d’entités qui peuvent être inclus dans le processus de consolidation : 

    room
    staff
    course
    module (matière en français)
    group
    student
    equip
    team
    faculty
    dept
    fixture (matériel en français)
    layout (type de salle en français)
    site
    eventCat
    supervisor
    staffCat
    user

## Pipelines (chaînes de traitement)

La section Pipelines sert à activer ou désactiver l'utilisation d'un thread unique durant certaines phases spécifiques du processus.

## Autres
Les autres éléments comprennent : 

**commandTimeouts** - utilisé pour spécifier le délai d’expiration de commande en millisecondes pour les bases de données sources, admin et public.

**forceRebuild** - impose une reconstruction complète des bases de données admin et public.

**maxDegreeOfParallelism** - nombre maximum de cores à utiliser dans le traitement parallèle.

## Journalisation (logging)

Le fichier CTDataStore.exe.config contient la configuration de la journalisation. Par défaut, un journal continu appelé “verto.txt” (verto était le nom initial du projet) est stocké dans le sous-dossier “logs”. Le niveau de journalisation par défaut est “WARN” mais vous pouvez le définir sur ALL, DEBUG, INFO, WARN, ERROR, FATAL ou OFF. Il suffit pour cela d’ouvrir le fichier CTDataStore.exe.config dans un éditeur de texte et d’apporter les modifications nécessaires.

