using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using GTProyect.Pages;
using Npgsql;
using NpgsqlTypes;

namespace GTProyect.Pages
{
    public partial class Index : System.Web.UI.Page
    {
        // Usa la cadena de conexion
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Lógica para cargar datos iniciales 
                if (UsuarioAutenticado())
                {
                    CargarPaises();
                    CargarTabla();
                    CargarPaisesEnDropDownList();
                }
                else
                {
                    Response.Redirect("Login.aspx");
                }
            }
            CargarPaisesEnDropDownList();
        }
        private void CargarPaisesEnDropDownList()
        {
            // Obtener la lista de países
            List<string> paises = ObtenerTodosLosPaises();

            DropDownListAgregar.DataSource = paises;
            DropDownListAgregar.DataBind();
            ddlCountry.DataSource = paises;
            ddlCountry.DataBind();
            // Agregar por defecto el todos
            DropDownListAgregar.Items.Insert(0, new ListItem("Todos", ""));
        }
        protected void pageIndex(object sender, GridViewPageEventArgs e)
        {
            keywordslist.PageIndex = e.NewPageIndex;

            CargarTabla();
        }
        private bool UsuarioAutenticado()
        {
            // Verificar si la variable de sesión "UsuarioAutenticado" está presente y es verdadera
            return Session["UsuarioAutenticado"] != null && (bool)Session["UsuarioAutenticado"];
        }
        private void CambiarNombreColumna(DataTable dt, string nombreAntiguo, string nombreNuevo)
        {
            if (dt.Columns.Contains(nombreAntiguo))
            {
                dt.Columns[nombreAntiguo].ColumnName = nombreNuevo;
            }
        }
        protected void ddlFiltroStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            string textoIngresado = tbSearch.Text.Trim(); ;
            CargarTabla(textoIngresado);
        }
        protected void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // Limpio estas variables de sesion en caso de agregar
                Session["SelectedKeyword"] = null;
                Session["SelectedName"] = null;
                Session["SelectedCountry"] = null;
                // Voy al crud
                Response.Redirect("CRUD.aspx");
            }
            catch (Exception ex)
            {
                Response.Write("Error al redirigir: " + ex.Message);
            }
        }
        void CargarTabla()
        {
            try
            {
                string selectedPais = ddlFiltroPaises.SelectedValue;
                string selectedStatus = ddlFiltroStatus.SelectedValue;

                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    string sql = "SELECT k.kw, k.activo, p.nombre FROM \"IA_GTRENDS\".testkeywords k INNER JOIN \"IA_GTRENDS\".pais p ON k.codp = p.codp WHERE (@selectedPais = 'Todos' OR p.nombre = @selectedPais) AND " +
                         "(@status = 'Todos' OR (k.activo = true AND @status = 'Activo') OR (k.activo = false AND @status = 'NoActivo')) ORDER BY k.kw";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@selectedPais", selectedPais);
                        cmd.Parameters.AddWithValue("@status", selectedStatus);

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            // Cambio nombre de columnas
                            CambiarNombreColumna(dt, "kw", "Palabra clave");
                            CambiarNombreColumna(dt, "activo", "Status");
                            CambiarNombreColumna(dt, "nombre", "Pais");

                            // Asigna la tabla de datos al GridView
                            keywordslist.DataSource = dt;
                            keywordslist.DataBind();
                        }
                    }
                    // Esto es para evitar que se me agreguen en bucle el Todos. Lo elimino si existe
                    if (ddlFiltroPaises.Items.FindByValue("Todos") != null)
                    {
                        ddlFiltroPaises.Items.Remove(ddlFiltroPaises.Items.FindByValue("Todos"));
                    }

                    // Aqui agrego la opcion todos
                    ddlFiltroPaises.Items.Insert(0, new ListItem("Todos", "Todos"));
                }
            }
            catch (Exception ex)
            {

                Response.Write("Error general: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Response.Write("<br />Inner Exception: " + ex.InnerException.Message);
                }
            }
        }
        void CargarPaises()
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT nombre FROM \"IA_GTRENDS\".pais order by pais", con))
                    {
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<string> paises = new List<string>();

                            while (reader.Read())
                            {
                                // Revisa que las columnas se llamen igual
                                string nombre_pais = reader["nombre"].ToString();

                                // Verifica si el país ya existe en la lista
                                if (!paises.Contains(nombre_pais))
                                {
                                    paises.Add(nombre_pais);
                                }
                            }

                            // Elimina "Todos" si ya existe
                            if (paises.Contains("Todos"))
                            {
                                paises.Remove("Todos");
                            }

                            // Asigna la lista de países al DropDownList
                            ddlFiltroPaises.DataSource = paises;
                            ddlFiltroPaises.DataBind();
                        }
                    }
                }

                // Agrego el todos al final de la lista
                ddlFiltroPaises.Items.Insert(0, new ListItem("Todos", "Todos"));
            }
            catch (Exception ex)
            {
                Response.Write("Error al cargar países: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Response.Write("Inner Exception: " + ex.InnerException.Message);
                }
            }
        }
        protected void GridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Obtiene el valor actual de la columna "Status"
                bool status = Convert.ToBoolean(DataBinder.Eval(e.Row.DataItem, "Status"));

                // Obtiene el control CheckBox en la fila
                CheckBox checkBoxStatus = (CheckBox)e.Row.FindControl("CheckBoxStatus");

                // Configura el estado del CheckBox según el valor de "Status"
                checkBoxStatus.Checked = status;
            }
        }
        void CargarTabla(string textoIngresado)
        {
            try
            {
                string selectedPais = ddlFiltroPaises.SelectedValue;
                string selectedStatus = ddlFiltroStatus.SelectedValue;

                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    string sql = "SELECT k.kw, k.activo, p.nombre FROM \"IA_GTRENDS\".testkeywords k INNER JOIN \"IA_GTRENDS\".pais p ON k.codp = p.codp WHERE " +
                                  "(@textoIngresado IS NULL OR k.kw ILIKE @textoIngresado) AND " +
                                  "(@selectedPais = 'Todos' OR p.nombre = @selectedPais) AND " +
                                  "(@status = 'Todos' OR (k.activo = true AND @status = 'Activo') OR (k.activo = false AND @status = 'NoActivo')) ORDER BY k.kw";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@textoIngresado", string.IsNullOrEmpty(textoIngresado) ? (object)DBNull.Value : $"%{textoIngresado}%");
                        cmd.Parameters.AddWithValue("@selectedPais", selectedPais);
                        cmd.Parameters.AddWithValue("@status", selectedStatus);

                    }
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@selectedPais", selectedPais);
                        cmd.Parameters.AddWithValue("@status", selectedStatus);
                        // Agregar parámetro de texto de búsqueda
                        cmd.Parameters.AddWithValue("@textoIngresado", $"%{textoIngresado}%");
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            // Cambio nombre de columnas
                            CambiarNombreColumna(dt, "kw", "Palabra clave");
                            CambiarNombreColumna(dt, "activo", "Status");
                            CambiarNombreColumna(dt, "nombre", "Pais");

                            keywordslist.DataSource = dt;
                            keywordslist.DataBind();
                        }
                    }
                    // Esto es para evitar que se me agreguen en bucle el Todos. Lo elimino si existe
                    if (ddlFiltroPaises.Items.FindByValue("Todos") != null)
                    {
                        ddlFiltroPaises.Items.Remove(ddlFiltroPaises.Items.FindByValue("Todos"));
                    }
                    // Agrego la opción Todos
                    ddlFiltroPaises.Items.Insert(0, new ListItem("Todos", "Todos"));
                }
            }
            catch (Exception ex)
            {
                // Manejo general de errores
                Response.Write("Error general: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Response.Write("<br />Inner Exception: " + ex.InnerException.Message);
                }
            }
        }
        protected void ddlFiltroPaises_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedPais = ddlFiltroPaises.SelectedValue;
            Session["SelectedPais"] = selectedPais;
            string textoIngresado = tbSearch.Text.Trim();
            CargarTabla(textoIngresado);
        }
        //Esta se va cuando solucione lo del modal
        protected void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtiene la fila seleccionada en el GridView
                Button btnDelete = (Button)sender;
                GridViewRow row = (GridViewRow)btnDelete.NamingContainer;

                // Verifica si la fila es nula
                if (row != null)
                {
                    int rowI = row.RowIndex;
                    // Obtiene valores de las celdas en esa fila
                    string keyword = HttpUtility.HtmlDecode(keywordslist.Rows[rowI].Cells[0].Text);
                    string name = HttpUtility.HtmlDecode(keywordslist.Rows[rowI].Cells[1].Text);
                    string country = HttpUtility.HtmlDecode(keywordslist.Rows[rowI].Cells[2].Text);



                    string codp = ObtenerCodigoPais(country);

                    Debug.WriteLine($"keyword: {keyword}");
                    Debug.WriteLine($"codp: {codp}");

                    // Usa la conexión y comando dentro de bloques using para liberar recursos
                    using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                    {
                        con.Open();
                        string storedProcedure = "\"IA_GTRENDS\".cambiarstatus";
                        using (NpgsqlCommand cmd = new NpgsqlCommand(storedProcedure, con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_codp", codp);
                            cmd.Parameters.AddWithValue("@p_kw", keyword);


                            NpgsqlParameter outputParameter = new NpgsqlParameter("@p_resultado", NpgsqlDbType.Integer)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmd.Parameters.Add(outputParameter);
                            cmd.ExecuteNonQuery();
                            int resultado = Convert.ToInt32(outputParameter.Value);
                            if (resultado == -1)
                            {
                                Debug.WriteLine("Estado de la keyword actualizado exitosamente.");
                                CheckBox1.Checked = !CheckBox1.Checked;
                                CargarTabla();
                            }
                            else if (resultado == -2)
                            {
                                Debug.WriteLine("No se encontró la keyword.");
                                MostrarMensajeError("No se encontró la keyword.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Maneja cualquier excepción
                Debug.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                // Registra el error y muestra un mensaje al usuario              
                MostrarMensajeError($"Error: {ex.Message}");
            }
        }
        private void MostrarMensajeError(string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowErrorPopup", $"ShowErrorPopup('{mensaje}')", true);
        }
        // Obtengo codp segun nombre de pais
        private string ObtenerCodigoPais(string country)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT codp FROM \"IA_GTRENDS\".pais WHERE nombre = @Nombre";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Nombre", country);

                    object result = command.ExecuteScalar();
                    return result != null ? result.ToString() : string.Empty;
                }
            }
        }
        protected void BtnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtener el texto de búsqueda
                string textoIngresado = tbSearch.Text.Trim();

                // Llamada a la función CargarTabla con el filtro de búsqueda
                CargarTabla(textoIngresado);
            }
            catch (Exception ex)
            {
                Response.Write("No se han encontrado elementos: " + ex.Message);
            }
        }
        protected void tbSearch_TextChanged(object sender, EventArgs e)
        {

            string textoIngresado = tbSearch.Text.Trim();


            CargarTabla(textoIngresado);
        }
        //En realizacion todavia
        protected void keywordslist_Sorting(object sender, GridViewSortEventArgs e)
        {
            // Obtener el campo por el cual se desea ordenar
            string sortExpression = e.SortExpression;

            // Determinar el orden de clasificación (ascendente o descendente)
            string sortDirection = (ViewState["SortDirection"] as string) ?? "ASC";
            if (sortDirection.Equals("ASC"))
            {
                sortDirection = "DESC";
            }
            else
            {
                sortDirection = "ASC";
            }

            // Actualizar el estado de ordenamiento en la vista
            ViewState["SortDirection"] = sortDirection;

            // Llamar a la función para cargar la tabla con el nuevo orden
            CargarTabla(sortExpression, sortDirection);
        }
        void CargarTabla(string orderBy, string orderDirection)
        {
            try
            {
                string selectedPais = ddlFiltroPaises.SelectedValue;
                string selectedStatus = ddlFiltroStatus.SelectedValue;

                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    // Construye la consulta SQL con los parámetros de ordenamiento
                    string sql = $"SELECT k.kw, k.activo, p.nombre FROM \"IA_GTRENDS\".testkeywords k INNER JOIN \"IA_GTRENDS\".pais p ON k.codp = p.codp " +
                                 $"WHERE (@selectedPais = 'Todos' OR p.nombre = @selectedPais) AND " +
                                 "(@status = 'Todos' OR (k.activo = true AND @status = 'Activo') OR (k.activo = false AND @status = 'NoActivo')) " +
                                 $"ORDER BY {orderBy} {orderDirection}";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@selectedPais", selectedPais);
                        cmd.Parameters.AddWithValue("@status", selectedStatus);

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            // Cambio nombre de columnas
                            CambiarNombreColumna(dt, "kw", "Palabra clave");
                            CambiarNombreColumna(dt, "activo", "Status");
                            CambiarNombreColumna(dt, "nombre", "Pais");

                            // Asigna la tabla de datos al GridView
                            keywordslist.DataSource = dt;
                            keywordslist.DataBind();
                        }
                    }
                    // Esto es para evitar que se me agreguen en bucle el Todos. Lo elimino si existe
                    if (ddlFiltroPaises.Items.FindByValue("Todos") != null)
                    {
                        ddlFiltroPaises.Items.Remove(ddlFiltroPaises.Items.FindByValue("Todos"));
                    }
                    // Aqui agrego la opcion todos
                    ddlFiltroPaises.Items.Insert(0, new ListItem("Todos", "Todos"));
                }
            }
            catch (Exception ex)
            {
                // Manejo general de errores
                Response.Write("Error general: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Response.Write("<br />Inner Exception: " + ex.InnerException.Message);
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

                        // Ejecutar el procedimiento almacenado
                        cmd.ExecuteNonQuery();

                        // Obtener el resultado del parámetro de salida
                        int resultado = Convert.ToInt32(outputParameter.Value);

                        // Manejar el resultado
                        switch (resultado)
                        {
                            case -1:
                                lblMensajeAdd.Text = "Palabra clave insertada exitosamente.";
                                lblMensajeAdd.Visible = true;

                                string refreshScript = @"
                            setTimeout(function(){
                                $('#modalAnidadoAgregar').modal('hide');
                                CargarTabla();
                            }, 3000); 
                        ";
                                ScriptManager.RegisterStartupScript(this, this.GetType(), "RefreshScript", refreshScript, true);

                                break;
                            case -2:
                                LabelError.Text = "La palabra clave ya existe y se ha activado.";
                                LabelError.Visible = true;
                                break;
                            case -3:
                                LabelError.Text = "La palabra clave ya existe y está activa.";
                                LabelError.Visible = true;
                                break;
                            default:
                                LabelError.Text = $"Resultado inesperado: {resultado}";
                                LabelError.Visible = true;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LabelError.Text = "Error al intentar insertar palabra clave. Error: " + ex.Message;
                LabelError.Visible = true;
            }
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

        //-----------------------------------------------------------------------------------------------------------
        // Modal
        protected void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtener valores de los controles dentro del modal
                string keyword = tbkeyword.Text;
                string country = ddlCountry.SelectedValue;
                CheckBox1.Checked = chkEstado.Checked;

                // Realizar la operación de inserción
                if (country == "TODOS" || country == "Todos")
                {
                    {
                        // Obtener la lista de todos los países disponibles en tu tabla de países
                        List<string> allCountries = ObtenerTodosLosPaises();

                        // Realizar la operación de inserción para cada país
                        foreach (string c in allCountries)
                        {
                            // Obtener el código del país
                            string codp = ObtenerCodigoPais(c);

                            // Insertar un nuevo dato en la tabla keywords
                            InsertarKeyword(codp, keyword);
                        }
                    }
                    lblMensajeAdd.Text = "La palabra fue agregada con éxito para todos los países.";
                }
                else
                {
                    // Insertar un nuevo dato en la tabla keywords
                    string codp = ObtenerCodigoPais(country);
                    try
                    {
                        InsertarKeyword(codp, keyword);
                        lblMensajeAdd.Text = "La palabra fue agregada con éxito.";
                    }
                    catch (Exception ex)
                    {
                        lblMensajeAdd.Text = "La palabra NO fue agregada";
                    }


                }
                // Limpiar los campos del formulario
                TextBox1.Text = string.Empty;
                tbkeyword.Text = string.Empty;
                ddlCountry.SelectedValue= null;
                //DropDownListAgregar.SelectedIndex = 0;
                CheckBox1.Checked = false;
                lblMensajeAdd.Text = "";
            }
            catch (Exception ex)
            {
                lblMensajeAdd.Text = "Error al agregar la palabra clave: " + ex.Message;
                if (ex.InnerException != null)
                {
                    lblMensajeAdd.Text = "<br />Inner Exception: " + ex.InnerException.Message;
                }
            }

        }
        protected void BtnModal_Click(object sender, EventArgs e)
        {
            try
            {
                Button btnModal = (Button)sender;
                GridViewRow row = (GridViewRow)btnModal.NamingContainer;

                if (row != null)
                {
                    int rowIndex = row.RowIndex;

                    // Obtener los valores de las celdas en esa fila
                    string keyword = keywordslist.Rows[rowIndex].Cells[0].Text;
                    string country = keywordslist.Rows[rowIndex].Cells[2].Text;
                    bool status = keywordslist.Rows[rowIndex].Cells[1].Enabled; //((CheckBox)row.FindControl("CheckBox1")).Checked;

                    // Asignar los valores a los controles del modal
                    tbkeyword.Text = keyword;
                    ddlCountry.SelectedValue = country;
                    ddlCountry.Enabled = false;
                    chkEstado.Checked = status;
                    // Mostrar el modal
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "myModal", "$('#modalAnidado').modal('show');", true);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
        protected void BtnModal_Click_Edit(object sender, EventArgs e)
        {
            try
            {
                Button btnModal = (Button)sender;
                GridViewRow row = (GridViewRow)btnModal.NamingContainer;
                if (row != null)
                {
                    int rowIndex = row.RowIndex;

                    // Obtener los valores de las celdas en esa fila
                    string keyword = keywordslist.Rows[rowIndex].Cells[0].Text;
                    string country = keywordslist.Rows[rowIndex].Cells[2].Text;
                    bool status = ((CheckBox)row.FindControl("CheckBoxStatus")).Checked;
                    hiddenKeyword.Value =  keyword;
                    hiddenSelectedCountry.Value = country;
                    // Asignar los valores a los controles del modal
                    TextBox1.Text = keyword;
                    DropDownListAgregar.SelectedValue = country;
                    DropDownListAgregar.Enabled = false;
                    CheckBox1.Checked = status;
                    // Mostrar el modal
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "myModal", "$('#modalAnidadoAgregar').modal('show');", true);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        protected void BtnGuardar_EdicionClick(object sender, EventArgs e)
        {
            try
            {
                // Obtener valores de los controles dentro del modal
                string keyword = TextBox1.Text;
                string country =  ObtenerCodigoPais(hiddenSelectedCountry.Value); 
                bool chequeado = CheckBox1.Checked;

                Keyword original = new Keyword {
                    codp =country ,
                    kw = hiddenKeyword.Value,
                    activo = chequeado,
                    originalKw = hiddenKeyword.Value,
                    originalCodp = country
                };

                Keyword modificada = new Keyword
                {
                    codp = country,
                    kw = keyword,
                    activo = chequeado,
                    originalKw = keyword,
                    originalCodp = country
                };


                try
                {
                    ActualizarKeyword(original, modificada);
                    lblMensajeAdd.Text = "La palabra fue agregada con éxito.";
                }
                catch (Exception ex)
                {
                    lblMensajeAdd.Text = "La palabra NO fue agregada";
                }

                // Limpiar los campos del formulario
                TextBox1.Text = string.Empty;
                tbkeyword.Text = string.Empty;
                ddlCountry.SelectedValue= null;
                //DropDownListAgregar.SelectedIndex = 0;
                CheckBox1.Checked = false;
                lblMensajeAdd.Text = "";
            }

            catch (Exception ex)
            {
                lblMensajeAdd.Text = "Error al agregar la palabra clave: " + ex.Message;
                if (ex.InnerException != null)
                {
                    lblMensajeAdd.Text = "<br />Inner Exception: " + ex.InnerException.Message;
                }
            }

        }


        //protected void CargarDatosModal(GridViewRow row)
        //{
        //    try
        //    {
        //        if (row != null)
        //        {
        //            // Obtener los valores de las celdas en esa fila
        //            string keyword = row.Cells[0].Text;
        //            string country = row.Cells[2].Text;
        //            bool status = ((CheckBox)row.FindControl("CheckBoxStatus")).Checked;

        //            // Asignar los valores a los controles del modal
        //            tbkeyword.Text = keyword;
        //            chkEstado.Checked = status;

        //            // Buscar el elemento del DropDownList por su valor
        //            ListItem listItem = ddlCountry.Items.FindByValue(country);
        //            if (listItem != null)
        //            {

        //                // Seleccionar el país recuperado de la fila
        //                listItem.Selected = true;
        //            }
        //            // Mostrar el modal
        //            ScriptManager.RegisterStartupScript(this, this.GetType(), "myModal", "$('#modalAnidadoAgregar').modal('show');", true);
        //        }
        //        else
        //        {
        //            // Manejar el caso si la fila seleccionada es nula
        //            Response.Write("La fila seleccionada es nula.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Manejar excepciones específicas
        //        Response.Write("Error al cargar los datos en el modal: " + ex.Message);
        //    }
        //}
        private void ActualizarKeyword(Keyword originalKeyword, Keyword newKeyword)
        {
            try
            {
                if (originalKeyword == newKeyword) { LabelError.Text = "No has introducido cambios en la keyword"; }
                else
                {
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
                                    lblMensajeAdd.Text = "Operación de actualización completada con éxito.";
                                    break;
                                case -3:
                                    LabelError.Text = "No has realizado modificaciones en la keyword";
                                    break;
                                default:
                                    LabelError.Text = $"Resultado inesperado: {resultado}";
                                    break;
                            }
                        }
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowPopup", $"ShowPopup('{lblMensajeAdd.Text}')", true);
                    }
                }
                CargarPaises();
                CargarTabla();
                CargarPaisesEnDropDownList();
            }
            catch (Exception ex)
            {
                LabelError.Text = "Error durante la actualización: " + ex.Message;
                if (ex.InnerException != null)
                {
                    LabelError.Text += "<br />Inner Exception: " + ex.InnerException.Message;
                }
                //RegistrarErrorEnLog(ex);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowPopup", $"ShowPopup('{lblMensajeAdd.Text}')", true);
            }
        }

        private Keyword ObtenerKeywordOriginal(string codp, string keyword)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM \"IA_GTRENDS\".testkeywords WHERE codp = @codp AND kw = @keyword LIMIT 1";
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
        //---------
    }
}