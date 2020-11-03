# Setup database test on docker
# oy2ntsnzu5gyqfysk5yaxim4hazaxjfelcgvlfc2dukrki
# We use Sql Server for linux
Set-ExecutionPolicy -ExecutionPolicy Unrestricted
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=!#123Pwd@!" -p 1402:1433 --name fluentDb -h fluentDb -d hadem/fluentdb-mssql-linux:fluentDbMsSqlLinux