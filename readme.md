Task:
Create an Web application (framework version of your choice) front end + back end, having operations available:
- creating Notes (note is a short string with time of creation attached), dates must be stored in UTC, but user must see time in he's timezone.
- getting the List of Notes created by all users with date filter (start - end)
- getting the list of all IP addresses ever blocked in this Web App (who and when) Front end having 3 pages(or views) with simplest design for all of 3 operations.
No authentication
Limit the requests to the application with following rule:
if more than 300 requests comes from single IP address in a minute, give error response for this IP;Use SQL Server for storage, use DB-first approach

DB creation script:
Scripts\ddl.sql

Config file: 
appsettings.json