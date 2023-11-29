<!DOCTYPE html>

<html lang="es">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Título de la Página</title>
    <link rel="stylesheet" type="text/css" href="Style.css" />
</head>
<body>
    <form runat="server">
        <div class="d-block mx-auto" style="max-width: 800px; width: 100%;">
            <br />
            <div class="mx-auto text-center">
                <h2>Palabras claves</h2>
            </div>
            <br />
            <div class="container">
                <div class="row" style="margin-bottom: 20px;">
                    <div class="col-4">
                        <label for="ddlFiltroPaises" class="form-label">País:</label>
                        <select id="ddlFiltroPaises" runat="server" autopostback="True" onchange="ddlFiltroPaises_SelectedIndexChanged" class="form-select"></select>
                    </div>
                    <div class="col-4">
                        <label for="ddlFiltroStatus" class="form-label">Estado:</label>
                        <select id="ddlFiltroStatus" runat="server" autopostback="true" onchange="ddlFiltroStatus_SelectedIndexChanged" class="form-select">
                            <option value="Todos">Todos</option>
                            <option value="Activo">Activo</option>
                            <option value="NoActivo">No Activo</option>
                        </select>
                    </div>
                    <div class="col align-self-center text-center">
                        <button id="BtnAdd" runat="server" class="btn btn-info btn-sm" onclick="BtnAdd_Click">Add</button>
                    </div>
                </div>
            </div>
            <br />
            <div class="container row">
                <div class="table">
                    <asp:GridView runat="server" ID="keywordslist" class="table table-borderless table-hover">
                        <Columns>
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <button runat="server" onclick="BtnModify_Click" class="btn btn-info btn-sm">Modify</button>
                                    <button runat="server" onclick="BtnDelete_Click" class="btn btn-info btn-sm">Delete</button>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </form>

    <footer class="mt-auto text-center">
        <div class="container">
            <div class="row">
                <div class="col text-center">
                    <img src="Images/Logos.png" alt="Logo" class="img-fluid" />
                </div>
            </div>
        </div>
    </footer>
</body>
</html>


