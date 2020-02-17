# Configuration and Execution

The CTDataStore.exe program is used to maintain the Data Store. Specify your configuration in the **DataStoreConfig.xml** file and then execute the CTDataStore.exe to generate the data store. It may take a little while to initially create the data store, but subsequent operations should be much quicker (especially if little has changed in the source timetables).

A sample DataStoreConfig.xml file is shown below followed by a description of relevant sections:

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
The source section specifies the source timetable databases. Database connections to each timetable within the timetables section are configured using standard [connection strings](https://www.connectionstrings.com/sql-server/).

## Destination

The destination section refers to the location of the data store - both the ADMIN and PUBLIC databases. Again standard connection strings are used. You can locate source, admin and public databases on separate servers if required.

## Consolidation

The consolidation section describes how the various entity types are matched by the CTDS process. For example, the following entry indicates that rooms should be matched by unique name:

``` xml
<entry entity="room" column="unique_name" />
```

With this configuration specified, CTDS will automatically consolidate room records across multiple timetables combining all room record with unique name “A1”, for example. If a particular entity type has no entry in this section then records of that type are not consolidated. There follows a list of entity types that may be included in the consolidation process:

    room
    staff
    course
    module
    group
    student
    equip
    team
    faculty
    dept
    fixture
    layout
    site
    eventCat
    supervisor
    staffCat
    user

## Pipelines

The pipelines section is used to selectively enable/disable single threaded operation during specified phases of the process.

## Other
Other items include:

**commandTimeouts** - use to specify the command timeout in millisecs for source, admin and public databases.

**forceRebuild** - forces a complete rebuild of the admin and public databases.

**maxDegreeOfParallelism** - maximum number of cores to use in parallel processing.

## Logging

The CTDataStore.exe.config file contains logging configuration. By default it stores a rolling log in the “logs” subfolder called “verto.txt” (Verto was the working name of the project). The default logging level is “WARN” but you can set it to ALL, DEBUG, INFO, WARN, ERROR, FATAL or OFF. Just open the CTDataStore.exe.config file in a text editor and make the necessary changes.

