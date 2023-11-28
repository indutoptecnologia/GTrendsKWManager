using System;
using System.Collections.Generic;
using System.Configuration; // Asegúrate de importar esta referencia
using System.Data;
using System.Web.UI.WebControls;
using Npgsql;
using NpgsqlTypes;

namespace GTProyect.Pages
{
    public partial class Index : System.Web.UI.Page
    {
        // Utiliza la cadena de conexión desde el archivo de configuración
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["TuConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarPaises();
                CargarTabla();
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

                    // Utiliza una consulta directa en lugar de un procedimiento almacenado
                    using (NpgsqlCommand cmd = new NpgsqlCommand($"SELECT kw, activo, nombre_pais FROM \"IA_GTRENDS\".Listarkw('{selectedPais}') WHERE (@status = 'Todos' OR activo = (@status = 'Activo'))", con))
                    {
                        cmd.Parameters.AddWithValue("@status", selectedStatus);

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            // Cambia el nombre de las columnas según tu preferencia
                            CambiarNombreColumna(dt, "kw", "Palabra clave");
                            CambiarNombreColumna(dt, "activo", "Status");
                            CambiarNombreColumna(dt, "nombre_pais", "Pais");

                            keywordslist.DataSource = dt;
                            keywordslist.DataBind();
                        }
                    }
                }

                // Eliminar "Todos" si ya existe en el DropDownList antes de agregarlo nuevamente
                if (ddlFiltroPaises.Items.FindByValue("Todos") != null)
                {
                    ddlFiltroPaises.Items.Remove(ddlFiltroPaises.Items.FindByValue("Todos"));
                }

                // Agregar una opción para "Todos" después de cargar la tabla
                ddlFiltroPaises.Items.Insert(0, new ListItem("Todos", "Todos"));
            }
            catch (Exception ex)
            {
                Response.Write("Error al cargar la tabla: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Response.Write("Inner Exception: " + ex.InnerException.Message);
                }
            }
        }
        protected void ddlFiltroStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarTabla();
        }


        // Método para cambiar el nombre de una columna en un DataTable
        private void CambiarNombreColumna(DataTable dt, string nombreAntiguo, string nombreNuevo)
        {
            if (dt.Columns.Contains(nombreAntiguo))
            {
                dt.Columns[nombreAntiguo].ColumnName = nombreNuevo;
            }
        }

        protected void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // Redirige a la página CRUD.aspx
                Response.Redirect("CRUD.aspx");
            }
            catch (Exception ex)
            {
                Response.Write("Error al redirigir: " + ex.Message);
            }
        }

        void CargarPaises()
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT nombre FROM \"IA_GTRENDS\".pais", con))
                    {
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<string> paises = new List<string>();

                            while (reader.Read())
                            {
                                // Revisar que las columnas se llamen igual
                                string nombre_pais = reader["nombre"].ToString();

                                // Verificar si el país ya existe en la lista
                                if (!paises.Contains(nombre_pais))
                                {
                                    paises.Add(nombre_pais);
                                }
                            }

                            // Eliminar "Todos" si ya existe
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

                // Agregar una opción para "Todos" después de asignar la fuente de datos al DropDownList
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




        protected void ddlFiltroPaises_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedPais = ddlFiltroPaises.SelectedValue;
            Session["SelectedPais"] = selectedPais;
            CargarTabla();
        }

        protected void BtnModify_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtener el botón que desencadenó el evento
                Button btnModify = (Button)sender;

                // Obtener la fila del GridView que contiene el botón
                GridViewRow row = (GridViewRow)btnModify.NamingContainer;

                // Obtener el valor de la columna "Keyword" en esa fila
                string keyword = ((Label)row.FindControl("LblKeyword")).Text;

                // Redirigir al CRUD con el parámetro "kw" y "modo" en la URL
                Response.Redirect($"CRUD.aspx?kw={keyword}&modo=Modificar");
            }
            catch (Exception ex)
            {
                Response.Write("Error al modificar: " + ex.Message);
            }
        }

        protected void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtener el botón que desencadenó el evento
                Button btnDelete = (Button)sender;

                // Obtener la fila que contiene el botón
                GridViewRow row = (GridViewRow)btnDelete.NamingContainer;

                // Obtener los valores necesarios desde los controles ASP.NET en la fila
                string keyword = ((TextBox)row.FindControl("tbkeyword")).Text;
                string country = ((DropDownList)row.FindControl("ddlCountry")).SelectedValue;

                // Realizar la operación de desactivar directamente en el evento BtnDelete_Click
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    // Construir la consulta SQL para actualizar el estado a inactivo (activo = 0)
                    string sql = "UPDATE keywords SET activo = 0 WHERE keyword = @kw AND nombrePais = @nombrePais";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@kw", keyword);
                        cmd.Parameters.AddWithValue("@nombrePais", country);

                        // Ejecutar la consulta SQL
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Actualizar la tabla después de la acción
                            CargarTabla();
                        }
                        else
                        {
                            // Manejar el caso en que no se encontró la keyword o no se realizó la actualización.
                            // lblMessage.Text = "No se encontró la keyword o no se realizó la actualización.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejar la excepción
                // lblMessage.Text = "Error: " + ex.Message;
            }
        }

       

        protected void HyperLinkModify_Click(object sender, EventArgs e)
        {
            // Obtén la keyword seleccionada desde el GridView
            string keyword = ObtenerValorSeleccionadoDesdeGridView();

            // Redirige a la página CRUD con el parámetro "modo" establecido en "Modificar"
            Response.Redirect($"CRUD.aspx?kw={keyword}&modo=Modificar");
        }

        private string ObtenerValorSeleccionadoDesdeGridView()
        {
            foreach (GridViewRow row in keywordslist.Rows)
            {
                RadioButton rb = row.FindControl("RbSelect") as RadioButton;
                if (rb.Checked)
                {
                    return (row.FindControl("LblKeyword") as Label).Text;
                }
            }
            return null;
        }
    }
}
