<%@ Page Title="" Language="C#" MasterPageFile="~/PP.Master" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="GTProyect.Pages.Index" EnableEventValidation="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="tittle" runat="server">
    GT Keyword Manager
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="Style.css" />
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/css/bootstrap.min.css" />
    <script src="https://code.jquery.com/jquery-3.2.1.slim.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.12.9/umd/popper.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/js/bootstrap.min.js"></script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="body" runat="server" class="text-center">
    <div class="d-block mx-auto" style="max-width: 1000px; width: 100%; border-radius:10px">
        <br />
        <div class="mx-auto text-center text-dark">
           <h2 class="text-dark" style="color: #138496 !important; font-size: 36px; font-weight: bold; font-family: Averta,  mukta, sans-serif;">PALABRAS CLAVES</h2>
        </div>
        <br />
        <div class="container" id="index-container">
            <div class="row mb-3 align-items-center">
                <div class="col-8">
                    <asp:Label runat="server" ID="LblSearch" CssClass="form-label text-dark" Text="Buscar:" />
                    <asp:TextBox runat="server" ID="tbSearch" CssClass="form-control bg-dark text-light rounded" OnTextChanged="tbSearch_TextChanged" AutoPostBack="true" Placeholder="Introduce la palabra clave" />
                </div>
                <div class="col-4 text-center">
                    <asp:Button runat="server" ID="BtnSearch" CssClass="btn btn-info btn-rounded" Text="Buscar" OnClick="BtnSearch_Click" style="margin-top: 5px;" />
                </div>
                <div class="col-4">
                    <asp:Label runat="server" ID="LblFiltroPais" CssClass="form-label text-dark" Text="País:" />
                    <asp:DropDownList ID="ddlFiltroPaises" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlFiltroPaises_SelectedIndexChanged" CssClass="form-select bg-dark text-light rounded h-100"></asp:DropDownList>
                </div>
                <div class="col-4">
                    <asp:Label runat="server" ID="LblFiltroStatus" CssClass="form-label text-dark" Text="Estado:" />
                    <asp:DropDownList ID="ddlFiltroStatus" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlFiltroStatus_SelectedIndexChanged" CssClass="form-select bg-dark text-light rounded h-100">
                        <asp:ListItem Value="Todos">Todos</asp:ListItem>
                        <asp:ListItem Value="Activo">Activo</asp:ListItem>
                        <asp:ListItem Value="NoActivo">No Activo</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-4 text-center">
                    <asp:Button runat="server" ID="BtnAdd" CssClass="btn btn-info btn-rounded" Text="Agregar" OnClick="BtnAdd_Click" style="margin-top: 5px;" />
                    <asp:LinkButton runat="server" ID="BtnAddMod" CssClass="btn btn-info btn-rounded" Text="Agr.Mod" OnClick="BtnModal_Click" style="margin-top: 5px;" data-toggle="modal" data-target="#modalAnidadoAgregar"></asp:LinkButton>
                </div>
            </div>
        </div>
        <br />
        <div class="container row bg-gray p-3 rounded">
            <div class="table-responsive text-center" style="overflow-y: auto;"> 
                <asp:GridView runat="server" ID="keywordslist" CssClass="table table-borderless table-hover text-light rounded custom-pagination"
                    AutoGenerateColumns="false" PageSize="10" OnSorting="keywordslist_Sorting" AllowPaging="True" OnPageIndexChanging="pageIndex">
                    <Columns>
                        <asp:BoundField DataField="Palabra Clave" HeaderText="PALABRA CLAVE" SortExpression="kw" ItemStyle-Width="200px" />
                        <asp:TemplateField HeaderText="ACTIVO" SortExpression="activo">
                            <ItemTemplate>
                                <asp:CheckBox ID="CheckBoxStatus" runat="server" Enabled="false" Checked='<%# Convert.ToBoolean(Eval("Status")) %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Pais" HeaderText="PAIS" SortExpression="nombre" ItemStyle-Width="200px" />
                        <asp:TemplateField HeaderText="ACCIONES" ItemStyle-Width="200px">
                            <ItemTemplate>
                                <asp:Button runat="server" ID="BtnModify" CssClass="btn btn-info btn-sm rounded" Text="Editar"   OnClick="BtnModify_Click" />
                                <asp:Button runat="server" ID="BtnDelete" CssClass="btn btn-info btn-sm rounded" Text="Estado" OnClick="BtnDelete_Click" />
                                <asp:Button runat="server" ID="BtnModal" Text="Modal" CssClass="btn btn-info btn-sm rounded" OnClick="BtnModal_Click" data-toggle="modal" data-target="#modalAnidado" CausesValidation="false"></asp:Button>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerStyle CssClass="custom-pagination" ForeColor="#138496" />
                </asp:GridView>
                <asp:Label runat="server" ID="Label1" CssClass="text-danger" Visible="false"></asp:Label>
            </div>
        </div>
    </div>
    <br />
     <!-- MODAL modificar -->
    <div class="modal fade" id="modalAnidado" tabindex="-1" role="dialog" aria-labelledby="modalLabel" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="modalLabel">Edición de Palabras</h5>
                    <button type="button" class="btn-close" data-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <!-- Formulario CRUD -->
                    <div class="mx-auto" style="max-width: 400px; width: 100%;">
                        <div class="mb-3">
                            <label class="form-label">Palabra clave</label>
                            <asp:TextBox runat="server" CssClass="form-control" ID="tbkeyword"></asp:TextBox>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">País</label>
                            <asp:DropDownList runat="server" CssClass="form-control" ID="ddlCountry" ></asp:DropDownList>
                        </div>
                        <div>
                            <label class="form-label">Activo</label>
                             <asp:CheckBox runat="server" ID="chkEstado" />
                        </div>                        
                        <asp:Button runat="server" CssClass="btn btn-primary" ID="BtnGuardar" Text="Guardar" OnClick="BtnGuardar_Click" BackColor="#138496" ForeColor="White"/>
                        <asp:Button runat="server" CssClass="btn btn-secondary" ID="BtnCerrar" Text="Cerrar" data-dismiss="modal" />
                    </div>                 
                    <asp:Label runat="server" ID="lblMensajeMod" CssClass="text-danger"></asp:Label>
                </div>
            </div>
        </div>
    </div>
    
<!-- MODAL agregar -->
<div class="modal fade" id="modalAnidadoAgregar" tabindex="-1" role="dialog" aria-labelledby="modalLabel" aria-hidden="true">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="modalLabelAdd">Administrador de palabras</h5>
                <button type="button" class="btn-close" aria-label="Close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <!-- Formulario CRUD -->
                <div class="mx-auto" style="max-width: 400px; width: 100%;">
                    <div class="mb-3">
                        <label class="form-label">Palabra clave</label>
                        <asp:TextBox runat="server" CssClass="form-control" ID="TextBox1"></asp:TextBox>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">País</label>
                        <asp:DropDownList runat="server" CssClass="form-control" ID="DropDownListAgregar" ></asp:DropDownList>
                    </div>
                    <div>
                        <label class="form-label">Activo</label>
                        <asp:CheckBox runat="server" ID="CheckBox1" />
                    </div>                        
                    <asp:Button runat="server" CssClass="btn btn-primary" ID="Button1" Text="Guardar" OnClick="BtnGuardar_Click" BackColor="#138496" ForeColor="White" ClientIDMode="Static" />
                    <asp:Button runat="server" CssClass="btn btn-secondary" ID="Button2" Text="Cerrar" data-dismiss="modal" />
                </div>  
                <asp:Label runat="server" ID="LabelError" CssClass="text-danger" Visible="false"></asp:Label>               
                <asp:Label runat="server" ID="lblMensajeAdd" CssClass="text-success" Visible="false"></asp:Label>   
            </div>
        </div>
    </div>
 
</div>

</asp:Content>
