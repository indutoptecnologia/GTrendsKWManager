using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Npgsql;

namespace GTProyect.Pages
{
    public partial class CRUD : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UsuarioAutenticado())
            {
                // Si no está autenticado, redirigir al login
                Response.Redirect("Login.aspx");
                return; 
            }
            if (!IsPostBack)
            {
                CargarPaises();

                // Verificar si hay variables de sesión
                if (Session["SelectedKeyword"] != null && Session["SelectedCountry"] != null)
                {
                    // Recuperar los valores de las variables de sesión
                    string selectedKeyword = Session["SelectedKeyword"].ToString();
                    string selectedCountry = Session["SelectedCountry"].ToString();

                    // Preconfigurar los campos del formulario con los valores recuperados
                    tbkeyword.Text = selectedKeyword;

                    // Verificar si el valor seleccionado está en la lista de la DropDownList
                    if (selectedCountry != null)
                    {
                        ddlCountry.SelectedValue = selectedCountry;
                    }

                    // Desactivar el botón BtnConfirm y activar BtnModify
                    BtnConfirm.Visible = false;
                    BtnModify.Visible = true;
                }
                else
                {
                    // si selectedkeyword y sc vienen nulas, es porque quiero agregar y no modificar.
                    BtnConfirm.Visible = true;
                    BtnModify.Visible = false;
                }
            }
        }
        private bool UsuarioAutenticado()
        {
            // Verificar si la variable de sesión "UsuarioAutenticado" existe y es true
            return Session["UsuarioAutenticado"] != null && (bool)Session["UsuarioAutenticado"];
        }
        protected void BtnConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtener valores de campos ocultos
                string keyword = hdnKeyword.Value;
                string selectedCountry = hdnSelectedCountry.Value;

                // Si el país seleccionado es "Todos", agrega para todos
                if (selectedCountry == "Todos")
                {
                    // Obtener la lista de todos los países disponibles en tu tabla de países
                    List<string> allCountries = ObtenerTodosLosPaises();

                    // Realizar la operación de inserción para cada país
                    foreach (string country in allCountries)
                    {
                        // Obtener el código del país
                        string codp = ObtenerCodigoPais(country);

                        // Insertar un nuevo dato en la tabla keywords
                        InsertarKeyword(codp, keyword);
                    }
                }
                else
                {
                    // Si se selecciona un país específico, realiza la inserción solo para ese país
                    string codp = ObtenerCodigoPais(selectedCountry);
                    InsertarKeyword(codp, keyword);
                }               
                lblMessage.Text = "La kw fue agregada con éxito.";
                
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowPopup", $"ShowPopup('{lblMessage.Text}')", true);
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error al agregar la kw: " + ex.Message;
                if (ex.InnerException != null)
                {
                    lblMessage.Text += "<br />Inner Exception: " + ex.InnerException.Message;
                }
            }
        }

        // Método para obtener todos los países disponibles en la tabla de países
        private List<string> ObtenerTodosLosPaises()
        {
            List<string> paises = new List<string>();

            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                con.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT nombre FROM \"IA_GTRENDS\".pais", con))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string nombre_pais = reader["nombre"].ToString();
                            paises.Add(nombre_pais);
                        }
                    }
                }
            }

            return paises;
        }

        // Método para obtener el código de un país específico
        private string ObtenerCodigoPais(string country)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT codp FROM \"IA_GTRENDS\".pais WHERE nombre = @Nombre";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Nombre", country);
                    return (string)command.ExecuteScalar();
                }
            }
        }

        // Método para insertar una keyword en la base de datos
        private void InsertarKeyword(string codp, string keyword)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string insertQuery = "INSERT INTO \"IA_GTRENDS\".keywords (codp, kw, activo) VALUES (@Codp, @Keyword, true)";
                using (NpgsqlCommand insertCommand = new NpgsqlCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@Codp", codp);
                    insertCommand.Parameters.AddWithValue("@Keyword", keyword);
                    insertCommand.ExecuteNonQuery();
                }
            }
        }

        protected void CargarPaises()
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    // Utilizar una consulta SQL para obtener los nombres de los países
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT nombre FROM \"IA_GTRENDS\".pais", con))
                    {
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<string> paises = new List<string>();

                            while (reader.Read())
                            {
                                // Revisar que las columnas se llamen igual
                                if (reader["nombre"] != null && reader["nombre"] != DBNull.Value)
                                {
                                    string nombre_pais = reader["nombre"].ToString();

                                    // Verificar si el país ya existe en la lista
                                    if (!paises.Contains(nombre_pais))
                                    {
                                        paises.Add(nombre_pais);
                                    }
                                }
                            }

                            // Eliminar "Todos" si ya existe
                            if (paises.Contains("Todos"))
                            {
                                paises.Remove("Todos");
                            }

                            // Asigna la lista de países al DropDownList
                            ddlCountry.DataSource = paises;
                            ddlCountry.DataBind();
                        }
                    }
                }
                // Agregar una opción para "Todos" después de asignar la fuente de datos al DropDownList
                ddlCountry.Items.Insert(0, new ListItem("Todos", "Todos"));
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error al cargar países: " + ex.Message;
                if (ex.InnerException != null)
                {
                    lblMessage.Text += "<br />Inner Exception: " + ex.InnerException.Message;
                }
            }
        }

 protected void BtnModify_Click(object sender, EventArgs e)
{
    try
    {
        // Obtener valores de sesión
        string keyword = Session["SelectedKeyword"].ToString();
        string selectedCountry = Session["SelectedCountry"].ToString();

        string codp = ObtenerCodigoPais(selectedCountry);

        Keyword originalKeyword = ObtenerKeywordOriginal(codp, keyword);

        if (originalKeyword != null)
        {
            string originalKw = originalKeyword.kw.Trim();
            string nuevaKw = tbkeyword.Text.Trim();
            string nuevoPais = ddlCountry.SelectedValue; 
            string newCodp = ObtenerCodigoPais(nuevoPais); 

            // Agrega mensajes de depuración
            Console.WriteLine($"Original Codp: {originalKeyword.codp}");
            Console.WriteLine($"New Codp: {newCodp}");
            Console.WriteLine($"Original Kw: {originalKeyword.originalKw}");
            Console.WriteLine($"New Kw: {nuevaKw}");

            // Crea un nuevo objeto Keyword con la nueva palabra clave y el nuevo código de país
            Keyword newKeyword = new Keyword
            {
                kw = nuevaKw,
                codp = newCodp,
                originalKw = originalKeyword.originalKw 
            };

            // Pasa el nuevo objeto Keyword al método ActualizarKeyword
            ActualizarKeyword(originalKeyword, newKeyword);

            lblMessage.Text = "La kw fue modificada con éxito.";
        }
        else
        {
            lblMessage.Text = "No se encontró el registro original en la base de datos.";
        }
    }
    catch (Exception ex)
    {
        lblMessage.Text = "Error al modificar la kw: " + ex.Message;
        if (ex.InnerException != null)
        {
            lblMessage.Text += $"<br />Inner Exception: {ex.InnerException.Message}";
        }
    }
    finally
    {
        ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowPopup", $"ShowPopup('{lblMessage.Text}')", true);
    }
}

        private void RegistrarErrorEnLog(Exception ex)
        {          
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
        }

        private Keyword ObtenerKeywordOriginal(string codp, string keyword)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM \"IA_GTRENDS\".keywords WHERE codp = @codp AND kw = @keyword LIMIT 1";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@codp", codp);
                    command.Parameters.AddWithValue("@keyword", keyword);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Keyword originalKeyword = new Keyword
                            {
                                codp = reader["codp"].ToString(),
                                kw = reader["kw"].ToString(),
                                activo = reader["activo"] != DBNull.Value && Convert.ToBoolean(reader["activo"]),
                                originalKw = reader["kw"].ToString(), 
                                originalCodp = reader["codp"].ToString() 
                            };

                            return originalKeyword;
                        }
                        else
                        {                          
                            System.Diagnostics.Debug.WriteLine($"No se encontró el registro con codp = {codp} y kw = {keyword}.");
                        }
                    }
                }
            }
            return null;
        }


        private void ActualizarKeyword(Keyword originalKeyword, Keyword newKeyword)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Actualizar tanto la palabra clave (kw) como el código de país (codp)
                    string updateQuery = "UPDATE \"IA_GTRENDS\".keywords SET kw = @KeywordValue, codp = @NewCodp WHERE codp = @Codp AND kw = @OriginalKeywordValue";

                    using (NpgsqlCommand updateCommand = new NpgsqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@KeywordValue", newKeyword.kw);
                        updateCommand.Parameters.AddWithValue("@NewCodp", newKeyword.codp); 
                        updateCommand.Parameters.AddWithValue("@Codp", originalKeyword.originalCodp); 
                        updateCommand.Parameters.AddWithValue("@OriginalKeywordValue", originalKeyword.originalKw);

                        updateCommand.ExecuteNonQuery();
                    }

                    // Mensaje de éxito si no hay errores
                    lblMessage.Text = "Operación de actualización completada con éxito.";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowPopup", $"ShowPopup('{lblMessage.Text}')", true);
                }
            }
            catch (Exception ex)
            {
                // Manejar errores
                lblMessage.Text = "Error durante la actualización: " + ex.Message;
                if (ex.InnerException != null)
                {
                    lblMessage.Text += "<br />Inner Exception: " + ex.InnerException.Message;
                }

                // Imprimir información clave para depuración
                Console.WriteLine($"Original Codp: {originalKeyword.originalCodp}");
                Console.WriteLine($"New Codp: {newKeyword.codp}");
                Console.WriteLine($"Original Kw: {originalKeyword.originalKw}");
                Console.WriteLine($"New Kw: {newKeyword.kw}");

                
                RegistrarErrorEnLog(ex);

                // Mostrar el mensaje de error
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowPopup", $"ShowPopup('{lblMessage.Text}')", true);
            }
        }






    }
}
