# Interroger Data Store

Ce document contient des instructions SQL pour donner des exemples de requêtes types pour interroger CELCAT Data Store. Vous trouverez une description de la structure et du contenu de la banque de données. Un exemple de requête est donné lorsque cela est possible, suivi d’un extrait des résultats.

## Liste simple des salles

Cette requête simple illustre le fait que Data Store contient des données dénormalisées pouvant simplifier la production de rapports. Veuillez noter que le nom des départements et des sites sont disponibles sans jointures.

``` SQL
SELECT
 room_id,
 unique_name,
 dept_name,
 site_name
FROM RESOURCE.ROOM
```

_.room_id | _.unique_name | _.dept_name | _.site_name
--- | --- | --- | ---
1 | A117 | Biology | Abbey End
2 | A308 | Portuguese | Abbey End
3 | A735 | Human Nutrition | Abbey End
4 | A815|  Business and Management Studies | Abbey End

## Instances d’événements

Cette requête obtient toutes les instances d’évènements pour la matinée du 7 Septembre 2015. Le nom des salles est inclus via une simple jointure. Veuillez noter que l’id d’instance d’événement est un type de chaîne et comprend l’id d’événement suivi d’un trait d’union puis d’un entier représentant la semaine de l’événement dans l’emploi du temps originel.

``` SQL
SELECT
 e.event_instance_id,
 e.start_time,
 e.end_time,
 e.event_cat_name,
 e.dept_name,
 er.room_unique_name
FROM EVENT.EVENT_INSTANCE e
LEFT OUTER JOIN EVENT.EVENT_ROOM er
 ON e.event_instance_id = er.event_instance_id
WHERE e.start_time >= '2015-09-07 09:00'
AND e.end_time <= '2015-09-07 12:00'
```

_.event_instance_id | _.start_time | _.end_time | _.event_cat_name | _.dept_name | _.room_unique_name
--- | --- | --- | --- | --- | ---
4482-0 | 2015-09-07 11:00:00.000 | 2015-09-07 12:00:00.000 | Seminar | Italian |  Beeston Room
4719-0 | 2015-09-07 09:00:00.000 | 2015-09-07 10:30:00.000 | Seminar | German | Thetford Room
4468-0 | 2015-09-07 09:00:00.000 | 2015-09-07 10:15:00.000 | Symposium |Computing | Allington Room
4723-0 | 2015-09-07 09:00:00.000 | 2015-09-07 10:00:00.000 | Symposium | Education | Lewes Room
4692-0 | 2015-09-07 10:00:00.000 | 2015-09-07 11:00:00.000 | Seminar | Design | Thames Room

## Attendance

Cette requête obtient des informations complètes sur le registre d’une instance d’événement donnée. Veuillez noter que la table REGISTER_MARK n’a pas de correspondance directe à une seule table de base de données CT7 – elle est générée par CTDS à partir de plusieurs tables sources de CT7 afin de simplifier l’interrogation des données d’Attendance. La requête suivante requiert des jointures entre plusieurs tables dans le schéma CT7 (y compris les tables d’exception d’Attendance). 

``` SQL
--LP COMBINE ATTENDANCE DATA FROM CT_AT_STUDENT_MARK, CT_AT_STUDENT_REGISTER, CT_AT_EXCEPTION

SELECT sm.student_id, sm.event_id, sm.week, sm.mark_id, sm.mins_late,
sm.source
FROM CT_AT_STUDENT_MARK sm

UNION ALL

SELECT sr.student_id, sr.event_id, sr.week, e.mark_id, e.mins_late, e.type
FROM CT_AT_STUDENT_REGISTER sr
LEFT OUTER JOIN CT_AT_EXCEPTION e
ON sr.exception_id = e.exception_id
WHERE NOT EXISTS
(SELECT 1 FROM CT_AT_STUDENT_MARK
WHERE CT_AT_STUDENT_MARK.student_id = sr.student_id
AND CT_AT_STUDENT_MARK.event_id = sr.event_id
AND CT_AT_STUDENT_MARK.week = sr.week)

SELECT
 student_unique_name,
 mark_name
FROM ATTENDANCE.REGISTER_MARK
WHERE event_instance_id = '2496-5'
ORDER BY student_unique_name
```
_.student_unique_name | _.mark_name
--- | ---
Dougherty, Barbara | Present
Godsall, Tracey | Present
Holmes, Lisa | Late (authorised)
Hullah, Kathleen | Present
Potts, Patricia | Present
Price, Michelle | Withdrawn
