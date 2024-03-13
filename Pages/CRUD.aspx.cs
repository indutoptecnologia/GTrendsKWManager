using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Npgsql;
using NpgsqlTypes;

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
                    string selectedKeyword = HttpUtility.HtmlDecode(Session["SelectedKeyword"].ToString());
                    tbkeyword.Text = selectedKeyword;

                    // Verificar si el valor que se selecciona es una cadena y no nulo
                    if (Session["SelectedCountry"] is string selectedCountryString)
                    {
                        // Dividir la cadena por comas
                        string[] selectedCountries = selectedCountryString.Split(',');
                        // Limpiar selecciones anteriores
                        ddlCountry.ClearSelection();
                        // Seleccionar los países recuperados de las variables de sesión
                        foreach (string country in selectedCountries)
                        {
                            ListItem item = ddlCountry.Items.FindByValue(country);
                            if (item != null)
                            {
                                item.Selected = true;
                            }
                        }
                        // Desactivar el botón BtnConfirm y activar BtnModify
                        BtnConfirm.Visible = false;
                        BtnModify.Visible = true;                   
                        ddlCountry.Attributes.Add("disabled", "disabled");
                    }
                    else
                    {
                        // Manejar el caso si SelectedCountry no es una cadena
                        lblMessage.Text = "Error: SelectedCountry no es una cadena válida.";
                        BtnConfirm.Visible = false;
                        BtnModify.Visible = false;
                    }
                }
                else
                {
                    // Si selectedkeyword y sc vienen nulas, es porque quiero agregar y no modificar.
                    BtnConfirm.Visible = true;
                    BtnModify.Visible = false;
                    // Activar la selección múltiple en modo de agregar
                    ddlCountry.SelectionMode = ListSelectionMode.Multiple;
                }
            }
        }
        private bool UsuarioAutenticado()
        {           
            return Session["UsuarioAutenticado"] != null && (bool)Session["UsuarioAutenticado"];
        }      
        private List<string> ObtenerTodosLosPaises()
        {
            List<string> paises = new List<string>();

            using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
            {
                con.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT nombre FROM \"IA_GTRENDS\".pais ORDER BY nombre", con))
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
        protected void BtnConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtener valores de campos ocultos
                string keyword = hdnKeyword.Value;
                // Verificar si la opción seleccionada es "Todos"
                if (ddlCountry.SelectedValue == "TODOS")
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
                    lblMessage.Text = "La palabra fue agregada con éxito para todos los países.";
                    // Redirigir después de 2 segundos
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "RedirectScript", "setTimeout(function() { window.location.href = 'index.aspx'; }, 2000);", true);
                }
                else
                {
                    // Verificar si hay al menos un país seleccionado
                    if (ddlCountry.GetSelectedIndices().Length > 0)
                    {
                        // Obtener la lista de países seleccionados
                        List<string> selectedCountries = new List<string>();
                        foreach (int index in ddlCountry.GetSelectedIndices())
                        {
                            selectedCountries.Add(ddlCountry.Items[index].Value);
                        }
                        // Realizar la operación de inserción para cada país seleccionado
                        foreach (string country in selectedCountries)
                        {
                            // Obtener el código del país
                            string codp = ObtenerCodigoPais(country);
                            // Insertar un nuevo dato en la tabla keywords
                            InsertarKeyword(codp, keyword);
                        }
                        lblMessage.Text = "La palabra fue agregada con éxito.";
                        // Redirigir después de 2 segundos
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "RedirectScript", "setTimeout(function() { window.location.href = 'index.aspx'; }, 2000);", true);
                    }
                    else
                    {
                        // Informar al usuario que debe seleccionar al menos un país
                        lblMessage.Text = "Por favor, seleccione al menos un país.";
                    }
                }
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
        private void InsertarKeyword(string codp, string keyword)
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    // Llamada al procedimiento almacenado
                    string storedProcedure = "\"IA_GTRENDS\".agregarpalabrasclave";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(storedProcedure, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        // Parámetros del procedimiento almacenado
                        cmd.Parameters.AddWithValue("p_codp", codp);
                        cmd.Parameters.AddWithValue("p_kw", keyword);
                        NpgsqlParameter outputParameter = new NpgsqlParameter("@p_resultado", NpgsqlDbType.Integer)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputParameter);
                        cmd.ExecuteNonQuery();

                        int resultado = Convert.ToInt32(outputParameter.Value);
                        switch (resultado)
                        {
                            case -1:                               
                                lblMessage.Text = "Palabra clave insertada exitosamente.";
                                ScriptManager.RegisterStartupScript(this, this.GetType(), "RedirectScript", "setTimeout(function() { window.location.href = 'index.aspx'; }, 1000);", true);
                                break;
                            case -2:                               
                                lblMessage.Text = "La palabra clave ya existe y se ha activado.";
                                ScriptManager.RegisterStartupScript(this, this.GetType(), "RedirectScript", "setTimeout(function() { window.location.href = 'index.aspx'; }, 1000);", true);
                                break;
                            case -3:                               
                                lblMessage.Text = "La palabra clave ya existe y está activa.";
                                break;
                            default:
                                lblMessage.Text = $"Resultado inesperado: {resultado}";
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error al intentar insertar palabra clave. Error: " + ex.Message;
            }
        }
        protected void CargarPaises()
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT nombre FROM \"IA_GTRENDS\".pais order by nombre", con))
                    {
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<string> paises = new List<string>();

                            while (reader.Read())
                            {
                                if (reader["nombre"] != null && reader["nombre"] != DBNull.Value)
                                {
                                    string nombre_pais = reader["nombre"].ToString();
                                    if (!paises.Contains(nombre_pais))
                                    {
                                        paises.Add(nombre_pais);
                                    }
                                }
                            }
                            if (paises.Contains("TODOS"))
                            {
                                paises.Remove("TODOS");
                            }
                            ddlCountry.DataSource = paises;
                            ddlCountry.DataBind();
                        }
                    }
                }
                // Agregar una opción para "Todos" 
                ddlCountry.Items.Insert(0, new ListItem("TODOS", "TODOS"));
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

                Keyword originalKeyword = ObtenerKeywordOriginal(codp, HttpUtility.HtmlDecode(keyword));

                if (originalKeyword != null)
                {
                    string originalKw = HttpUtility.HtmlDecode(originalKeyword.kw.Trim());
                    string nuevaKw = HttpUtility.HtmlDecode(tbkeyword.Text.Trim());
                    string nuevoPais = HttpUtility.HtmlDecode(ddlCountry.SelectedValue);

                    if (string.Equals(nuevaKw, HttpUtility.HtmlDecode(originalKw), StringComparison.OrdinalIgnoreCase))
                    {
                        lblMessage.Text = "No estás ingresando modificaciones en la keyword";
                    }
                    else
                    {
                        Keyword newKeyword = new Keyword
                        {
                            kw = nuevaKw,
                            originalKw = originalKeyword.originalKw
                        };

                        ActualizarKeyword(originalKeyword, newKeyword);

                        lblMessage.Text = "La palabra fue modificada con éxito.";
                        // Redirige después de 2 segundos
                        Response.AddHeader("REFRESH", "2;URL=index.aspx");
                    }
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
                if (originalKeyword == newKeyword) {lblMessage.Text = "No has introducido cambios en la keyword"; }
                else { 
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string storedProcedure = "\"IA_GTRENDS\".modificarPalabraClave";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(storedProcedure, connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("p_keywordvalue", newKeyword.kw);
                        cmd.Parameters.AddWithValue("p_codp", originalKeyword.originalCodp);
                        cmd.Parameters.AddWithValue("p_originalkeywordvalue", originalKeyword.originalKw);

                        NpgsqlParameter outputParameter = new NpgsqlParameter("@p_resultado", NpgsqlDbType.Integer)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputParameter);
                        cmd.ExecuteNonQuery();
                        int resultado = Convert.ToInt32(outputParameter.Value);
                        switch (resultado)
                        {
                           
                            case -1:
                                lblMessage.Text = "Operación de actualización completada con éxito.";                                
                                break;
                            case -3:
                                lblMessage.Text = "No has realizado modificaciones en la keyword";
                                break;
                            default:
                                lblMessage.Text = $"Resultado inesperado: {resultado}";
                                break;
                        }
                        }
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowPopup", $"ShowPopup('{lblMessage.Text}')", true);
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error durante la actualización: " + ex.Message;
                if (ex.InnerException != null)
                {
                    lblMessage.Text += "<br />Inner Exception: " + ex.InnerException.Message;
                }
                RegistrarErrorEnLog(ex);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowPopup", $"ShowPopup('{lblMessage.Text}')", true);
            }
        }
    }
}
