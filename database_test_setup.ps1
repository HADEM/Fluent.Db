# Setup database test on docker
# We use Sql Server for linux
Set-ExecutionPolicy -ExecutionPolicy Unrestricted
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=!#123Pwd@!" -p 1402:1433 --name fluentDb -h fluentDb -d mcr.microsoft.com/mssql/server:2019-latest