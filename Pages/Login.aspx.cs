using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient; // Ajusta el using según el proveedor de tu base de datos
using System.Web.UI;
using Npgsql;

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
                lblErrorMessage.Text = "Error: Usuario o contraseña incorrectos";
                lblErrorMessage.Visible = true;
            }
        }

        private bool ValidarCredenciales(string username, string password, out string mensajeError)
        {
            // Inicializa mensaje de error
            mensajeError = string.Empty;

            // Verificar campos vacios
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

                    // Consulto credenciales
                    string query = "SELECT COUNT(*) FROM \"IA_GTRENDS\".users WHERE username = @Username AND pass = @Password";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                    {
                        // Parámetros 
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);

                        // Resultado
                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        // Verifico que no haya campos vacios
                        if (count > 0)
                        {
                            return true;
                        }
                        else
                        {
                            mensajeError = "Credenciales incorrectas. Verifica tu nombre de usuario y contraseña.";
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Otros errores:
                mensajeError = "Error al intentar autenticar. Por favor, intenta nuevamente más tarde.";
                return false;
            }
        }

    }
}


