# CELCAT Data Store
The CELCAT Data Store (CTDS) is a data repository that provides end-users with access to consolidated timetable data, periodically updated from the source timetable databases. The product deliverables include ETL (Extract-Transform-Load) processes that operate on specified source timetables to produce internal, user-facing and hybrid data stores in normalised and denormalised form.

## Not a Warehouse

CTDS is not a Data Warehouse; rather it can form part of a data warehouse. Our rationale is that the Business Intelligence requirements of a large college or university will require a warehouse drawing data from the HR system, the student MIS system, the timetable, etc. CELCAT timetable data is just a small part of the overall picture. CTDS will provide the access layer needed to feed into such a warehouse.

This repository shares the source code of the application.

## Disclaimer

The source code shared here is created by various teams within CELCAT with different programming skills and responsibilities. Note that some of the code shared here may not reach the standards used by CELCAT's Development team in their official products.

## Copyright

Unless specified otherwise, the copyright for all the information shared here (including source code, instructions and documents) is described in the LICENSE file. Information provided by CELCAT is generally covered by the GNU General Public License version 3 (Note GNU not UNIX), but may incorporate information shared by other contributors who have elected to use a different license.

## Limitation of Liability

CELCAT is not liable for any damages caused by the use of information we have shared. More details are described in Section 15, 16 and 17 of the GNU General Public License version 3.

## Support

None of the shared information (including source code, instructions and documents) is considered as a product owned by CELCAT and is thus neither supported by the CELCAT Tech Support team nor subject to any Service Level Agreement. Any issues or feature requests may be raised on the Issues section of the GitHub website (which will require a GitHub account), however CELCAT cannot guarantee that it will respond, or take any action, to any issue or request raised.

## Introduction

Please see the documents in the Documentation folder.

## How to Use

* Clone or download the code repository to local.
* Open and build the solution with Visual Studio 2017 or later.
* Configure and execute the console app, TestClient, following the steps outlined in the configuration document (which can be found in the Documentation folder).