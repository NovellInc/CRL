namespace CRL_Publication.CustomAction
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using Microsoft.Deployment.WindowsInstaller;

    /// <summary>
    ///     Класс реализует пользовательские действия.
    /// </summary>
    public class CustomActions
    {
        /// <summary>
        ///     Проверяет подключение к MS SQL базе данных.
        /// </summary>
        /// <param name="session">Сессия процесса установки.</param>
        /// <returns>Возвращает результат действия - Success.</returns>
        [CustomAction]
        public static ActionResult VerifySqlConnection(Session session)
        {
            try
            {
                session.Log("VerifySqlConnection: Begin");

                var builder = new SqlConnectionStringBuilder
                {
                    DataSource = session["DATABASE_SERVER"],
                    InitialCatalog = "master",
                    ConnectTimeout = 5
                };

                if (session["DATABASE_LOGON_TYPE"] != "DatabaseIntegratedAuth")
                {
                    builder.UserID = session["DATABASE_USERNAME"];
                    builder.Password = session["DATABASE_PASSWORD"];
                }
                else
                {
                    builder.IntegratedSecurity = true;
                }

                using (var connection = new SqlConnection(builder.ConnectionString))
                {
                    try
                    {
                        connection.Open();
                        if (connection.State == ConnectionState.Open)
                        {
                            session["LOGON_VALID"] = "1";
                            session["DIALOG_MESSAGE"] = "Connection established.";
                        }

                        connection.Close();
                    }
                    catch (SqlException)
                    {
                        session["LOGON_VALID"] = string.Empty;
                        session["DIALOG_MESSAGE"] = "Connection not established. Check connection settings.";
                    }
                }
            }
            catch (Exception exception)
            {
                session.Log("VerifySqlConnection: exception: {0}", exception);
                return ActionResult.Failure;
            }

            return ActionResult.Success;
        }
    }
}