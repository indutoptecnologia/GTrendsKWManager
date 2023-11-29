<%@ Page Title="" Language="C#" MasterPageFile="~/PP.Master" AutoEventWireup="true" CodeBehind="CRUD.aspx.cs" Inherits="GTProyect.Pages.CRUD" %>
<asp:Content ID="Content1" ContentPlaceHolderID="tittle" runat="server">
    CRUD
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="Style.css" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="body" runat="server">
    <br />
    <div class="mx-auto" style="width:250px">
        <asp:Label runat="server" CssClass="h2" ID="lbltitulo"></asp:Label>
    </div>
    <div class="d-block mx-auto" style="max-width: 400px; width: 100%;">
        <div class="mb-3">
            <label class="form-label">Keyword</label>
            <asp:TextBox runat="server" CssClass="form-control" ID="tbkeyword"></asp:TextBox>
        </div>
        <div class="mb-3">
            <label class="form-label">Country</label>
            <asp:DropDownList runat="server" CssClass="form-control" ID="ddlCountry"></asp:DropDownList>
        </div>

        <div class="mb-3">
            <label class="form-label">Active for this country</label>
            <asp:CheckBox runat="server" CssClass="form-check-input" ID="cbStatusForCountry" />
        </div>
        <div class="mb-3">
            <label class="form-label">Active globally</label>
            <asp:CheckBox runat="server" CssClass="form-check-input" ID="cbStatusGlobally" />
        </div>

        <!-- Agregado para mostrar mensajes -->
        <asp:Label runat="server" ID="lblMessage" ForeColor="Red"></asp:Label>

        <script src="https://code.jquery.com/jquery-3.6.4.min.js"></script>
        <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js"></script>
        <script>
            function ShowPopup(message) {
                // Configura el contenido del modal
                $('#myModal .modal-body').text(message);

                // Muestra el modal
                $('#myModal').modal('show');
            }
        </script>

        <asp:Button runat="server" CssClass="btn btn-primary" ID="BtnAdd" Text="Add" Visible="false" />
        <asp:Button runat="server" CssClass="btn btn-primary" ID="BtnModify" Text="Modify" Visible="false" />
        <asp:Button runat="server" CssClass="btn btn-primary" ID="BtnDelete" Text="Delete" Visible="false" />
        <asp:Button runat="server" CssClass="btn btn-primary" ID="BtnSearch" Text="Search" Visible="false" />
        <asp:Button runat="server" CssClass="btn btn-primary btn-dif" ID="BtnCancel" Text="Cancel" Visible="true"
            PostBackUrl="~/Pages/Index.aspx" />

        <asp:Button runat="server" CssClass="btn btn-primary btn-dif" ID="BtnConfirm" Text="Confirm" Visible="true" OnClientClick="return ConfirmClick();" />

        <!-- Modal -->
        <div class="modal fade" id="myModal" tabindex="-1" aria-labelledby="exampleModalLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="exampleModalLabel">Mensaje</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <!-- El contenido del mensaje se llenará dinámicamente con JavaScript -->
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>

        <script>
            function ConfirmClick() {
                // Obtener el valor del keyword ingresado por el usuario
                var keyword = $('#<%= tbkeyword.ClientID %>').val();

                // Obtener el nombre del país seleccionado del DropDownList
                var selectedCountry = $('#<%= ddlCountry.ClientID %> option:selected').text();

               
                // Realizar la consulta para obtener el codp del país
                $.ajax({
                    type: "POST",
                    url: "CRUD.aspx/GetCountryCode",
                    data: JSON.stringify({ countryName: selectedCountry }),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (response) {
                        var codp = response.d;

                        // Insertar un nuevo dato en la tabla keywords
                        $.ajax({
                            type: "POST",
                            url: "CRUD.aspx/InsertKeyword",
                            data: JSON.stringify({ codp: codp, keyword: keyword, statusForCountry: statusForCountry, statusGlobally: statusGlobally }),
                            contentType: "application/json; charset=utf-8",
                            dataType: "json",
                            success: function (response) {
                                ShowPopup("La kw fue agregada con éxito.");
                            },
                            error: function (response) {
                                ShowPopup("Error al insertar la kw: " + response.responseText);
                            }
                        });
                    },
                    error: function (response) {
                        ShowPopup("Error al obtener el código del país: " + response.responseText);
                    }
                });

                return false; // Evitar que se realice el postback
            }
        </script>
    </div>
</asp:Content>


