<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Apps.aspx.cs" Inherits="LearningHub.Apps" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            
            <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" Height="100px" Width="300px" BackColor="White" BorderColor="#999999"
                BorderStyle="None" BorderWidth="1px" CellPadding="3" GridLines="Vertical" ShowFooter="true" 
                OnRowCancelingEdit="GridView1_RowCancelingEdit" OnRowDeleting="GridView1_RowDeleting" 
                OnRowEditing="GridView1_RowEditing" OnRowUpdating="GridView1_RowUpdating">
                <AlternatingRowStyle BackColor="#DCDCDC" />
                    <Columns>
                        <asp:TemplateField HeaderText="Name">
                            <ItemTemplate>
                                <asp:Label ID="LblName" runat="server" Text='<%# Bind("Name")%>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="TxtEditName" runat="server"></asp:TextBox>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:TextBox ID="TxtName" runat="server"></asp:TextBox>
                            </FooterTemplate>
                          </asp:TemplateField>

                          <asp:TemplateField HeaderText="Path">
                            <ItemTemplate>
                                <asp:Label ID="LblPath" runat="server" Text='<%# Bind("Path")%>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="TxtEditPath" runat="server"></asp:TextBox>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:TextBox ID="TxtPath" runat="server"></asp:TextBox>
                            </FooterTemplate>
                         </asp:TemplateField> 

                        <asp:TemplateField HeaderText="Remote">
                            <ItemTemplate>
                                <asp:Label ID="LblRemote" runat="server" Text='<%# Bind("Remote")%>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="TxtEditRemote" runat="server"></asp:TextBox>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:TextBox ID="TxtRemote" runat="server"></asp:TextBox>
                            </FooterTemplate>
                         </asp:TemplateField>

                        <asp:TemplateField HeaderText="TCPListener">
                            <ItemTemplate>
                                <asp:Label ID="LblTCPListener" runat="server" Text='<%# Bind("TCPListener")%>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="TxtEditTCPListener" runat="server"></asp:TextBox>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:TextBox ID="TxtTCPListener" runat="server"></asp:TextBox>
                            </FooterTemplate>
                         </asp:TemplateField>

                        <asp:TemplateField HeaderText="TCPSender">
                            <ItemTemplate>
                                <asp:Label ID="LblTCPSender" runat="server" Text='<%# Bind("TCPSender")%>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="TxtEditTCPSender" runat="server"></asp:TextBox>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:TextBox ID="TxtTCPSender" runat="server"></asp:TextBox>
                            </FooterTemplate>
                         </asp:TemplateField>

                        <asp:TemplateField HeaderText="UDPListener">
                            <ItemTemplate>
                                <asp:Label ID="LblUDPListener" runat="server" Text='<%# Bind("UDPListener")%>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="TxtEditUDPListener" runat="server"></asp:TextBox>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:TextBox ID="TxtUDPListener" runat="server"></asp:TextBox>
                            </FooterTemplate>
                         </asp:TemplateField>

                        <asp:TemplateField HeaderText="UDPSender">
                            <ItemTemplate>
                                <asp:Label ID="LblUDPSender" runat="server" Text='<%# Bind("UDPSender")%>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="TxtEditUDPSender" runat="server"></asp:TextBox>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:TextBox ID="TxtUDPSender" runat="server"></asp:TextBox>
                            </FooterTemplate>
                         </asp:TemplateField>

                        <asp:TemplateField HeaderText="Used">
                            <ItemTemplate>
                                <asp:Label ID="LblUsed" runat="server" Text='<%# Bind("Used")%>'></asp:Label>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="TxtEditUsed" runat="server"></asp:TextBox>
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:TextBox ID="TxtUsed" runat="server"></asp:TextBox>
                            </FooterTemplate>
                         </asp:TemplateField>

                        <asp:TemplateField HeaderText="Operations">
                            <ItemTemplate>
                                <asp:Button ID="BtnEdit" runat="server" CommandName="Edit" Text="Edit" />
                                <asp:Button ID="BtnDelete" runat="server" CommandName="Delete" Text="Delete" />
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:Button ID="BthUpdate" runat="server" CommandName="Update" Text="Update" />
                                <asp:Button ID="BtnCancel" runat="server" CommandName="Cancel" Text="Cancel" />
                            </EditItemTemplate>
                            <FooterTemplate>
                                <asp:Button ID="BtnInsert" runat="server" Text="Insert" OnClick="BtnInsert_Click" />
                            </FooterTemplate>
                        </asp:TemplateField>

                    </Columns>
                    <FooterStyle BackColor="#CCCCCC" ForeColor="Black" Font-Size="Small" />
                    <HeaderStyle BackColor="#000084" Font-Bold="True" ForeColor="White" Font-Size="Small" />
                    <PagerStyle BackColor="#999999" ForeColor="Black" HorizontalAlign="Center" Font-Size="Small" />
                    <RowStyle BackColor="#EEEEEE" ForeColor="Black" Font-Size="X-Small" />
                    <SelectedRowStyle BackColor="#008A8C" Font-Bold="True" ForeColor="White" Font-Size="Small" />
                    <SortedAscendingCellStyle BackColor="#F1F1F1"  />
                    <SortedAscendingHeaderStyle BackColor="#0000A9" />
                    <SortedDescendingCellStyle BackColor="#CAC9C9" />
                    <SortedDescendingHeaderStyle BackColor="#000065" />

                </asp:GridView>
                
        </div>
        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Start Applications" />
    </form>
</body>
</html>
