Verto is a collection of assemblies that implements the CELCAT Data Store (CTDS) - a data repository that provides end-users with access to consolidated CT7 and CT8 timetable data, periodically updated from the source timetable databases. 

The CTDS works like this:

1. Source Timetable databases are copied into a staging area in an ADMIN database. This is accomplished quickly using Sql Bulk Insert and has little performance impact on the source databases. Note that there are actually 3 staging schemas in the ADMIN database; "Primary", "Secondary" and "Temp". Source timetables are always copied to the Temp schema first.

2. When the timetables have been successfully copied into the Temp schema they are checked for validity. If all is well, the Primary schema is copied to Secondary and Temp to Primary. However if there are integrity issues the process is aborted leaving everything in a consistent state.

3. The Primary and Secondary schemas are diff'd to analyse what has changed in the latest extract. Changes are recorded in the ADMIN database's "History" schema.

4. Any newly added resources are assigned a "Federation ID" in the ADMIN database's "Federation" schema. This is where resources from multiple timetables are given a unique Id that is to be used in the public-facing CTDS.

5. "Consolidation Ids" are also generated in order to link together resources from different timetables that are to be treated as the same physical resource. This information is stored in the Federation schema and we refer to the collection of tables as the "consolidation map". See the DataStoreConfig.xml file, "consolidation" element for the basis on which resources are consolidated.

6. The History schema is updated to ensure that all Ids are actually federated ones rather than native timetable ones.

7. The newly generated rows in the History schema are then copied (using Bulk Insert) into the PUBLIC database's staging schema, along with the consolidation map.


