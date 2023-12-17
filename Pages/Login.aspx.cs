using System;
using System.Configuration;
using System.Data;
using System.Web.UI;
using Npgsql;
using NpgsqlTypes;

namespace GTProyect.Pages
{
    public partial class Login : System.Web.UI.Page
    {
        // Toma la cadena de conexión desde el archivo de configuración
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

        protected void BtnLogin_Click(object sender, EventArgs e)
        {
            // guarda user y pass ingresados
            string username = TxtUsername.Text;
            string password = TxtPassword.Text;
            string mensajeError;

            // Valida con la base
            if (ValidarCredenciales(username, password, out mensajeError))
            {
                // Ok? vamos al index, redirigir al Index

                Session["UsuarioAutenticado"] = true;
                Session["NombreUsuario"] = username;
                Response.Redirect("Index.aspx");
            }
            else
            {
                // Mal? error
                lblErrorMessage.Text = mensajeError;
                lblErrorMessage.Visible = true;
            }
        }

        private bool ValidarCredenciales(string username, string password, out string mensajeError)
        {
            // Inicializa mensaje de error
            mensajeError = string.Empty;
            

            // Verificar campos vacíos
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                mensajeError = "Por favor, completa todos los campos.";
                return false;
            }

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    // Llamada al procedimiento almacenado
                    string storedProcedure = "\"IA_GTRENDS\".conteoUsuarios";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(storedProcedure, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        // Parámetros del procedimiento almacenado
                        cmd.Parameters.AddWithValue("p_username", username);
                        cmd.Parameters.AddWithValue("p_pass", password);
                        
                       
                        // Parámetro de salida
                        NpgsqlParameter outputParameter = new NpgsqlParameter("p_resultado", NpgsqlDbType.Integer);
                        outputParameter.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outputParameter);
                        // Ejecutar el procedimiento almacenado
                        cmd.ExecuteNonQuery();
                        // Obtener el resultado del parámetro de salida
                        int resultado = (int)outputParameter.Value;
                        // Verificar el resultado
                        if (resultado > 0)
                        {
                            return true;
                        }
                        else
                        {
                            mensajeError = "Credenciales incorrectas. Verifica tu usuario y/o contraseña.";
                            return false;
                        }
                    }
                }
            }
           
                catch (Exception ex)
            {
                // Otros errores
                mensajeError = $"Error al intentar autenticar. Error: {ex.Message}";
                return false;
            }

        }



    }
}


