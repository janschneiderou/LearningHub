using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace LearningHub
{
    public partial class Apps : System.Web.UI.Page
    {
        public static Classes.Controller controller;
        string appsFile = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack == true)
            {
                Get_Xml();
            }
            //controller = new Classes.Controller(appsFile);

        }

        private void Get_Xml()
        {
            DataSet ds = new DataSet();

            ds.ReadXml(Server.MapPath("~/DataConfig/AppsConfig.xml"));
            appsFile = Server.MapPath("~/DataConfig/AppsConfig.xml");
            if (ds != null && ds.HasChanges())

            {
                GridView1.DataSource = ds;
                GridView1.DataBind();
            }
            else
            {
                GridView1.DataBind();
            }
        }

        protected void GridView1_RowEditing(object sender, GridViewEditEventArgs e)
        {
            GridView1.EditIndex = e.NewEditIndex;
            Get_Xml();
        }

        protected void GridView1_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            GridView1.EditIndex = -1;
            Get_Xml();
        }

        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            Get_Xml();
            DataSet ds = GridView1.DataSource as DataSet;
            ds.Tables[0].Rows[GridView1.Rows[e.RowIndex].DataItemIndex].Delete();
            ds.WriteXml(Server.MapPath("~/DataConfig/AppsConfig.xml"));

            Get_Xml();
        }

        protected void BtnInsert_Click(object sender, EventArgs e)
        {
            Insert_XML();
        }

        private void Insert_XML()
        {
            TextBox txtBox_Name = GridView1.FooterRow.FindControl("TxtName") as TextBox;
            TextBox txtBox_Path = GridView1.FooterRow.FindControl("TxtPath") as TextBox;
            TextBox txtBox_Remote = GridView1.FooterRow.FindControl("TxtRemote") as TextBox;
            TextBox txtBox_TCPListener = GridView1.FooterRow.FindControl("TxtTCPListener") as TextBox;
            TextBox txtBox_TCPSender = GridView1.FooterRow.FindControl("TxtTCPSender") as TextBox;
            TextBox txtBox_UDPListener = GridView1.FooterRow.FindControl("TxtUDPListener") as TextBox;
            TextBox txtBox_UDPSender = GridView1.FooterRow.FindControl("TxtUDPSender") as TextBox;
            TextBox txtBox_Used = GridView1.FooterRow.FindControl("TxtUsed") as TextBox;

            XmlDocument MyXmlDocument = new XmlDocument();
            MyXmlDocument.Load(Server.MapPath("~/DataConfig/AppsConfig.xml"));
            XmlElement ParentElement = MyXmlDocument.CreateElement("Application");
            XmlElement Name = MyXmlDocument.CreateElement("Name");
            Name.InnerText = txtBox_Name.Text;
            XmlElement Path = MyXmlDocument.CreateElement("Path");
            Path.InnerText = txtBox_Path.Text;
            XmlElement Remote = MyXmlDocument.CreateElement("Remote");
            Remote.InnerText = txtBox_Remote.Text;
            XmlElement TCPListener = MyXmlDocument.CreateElement("TCPListener");
            TCPListener.InnerText = txtBox_TCPListener.Text;
            XmlElement TCPSender = MyXmlDocument.CreateElement("TCPSender");
            TCPSender.InnerText = txtBox_TCPSender.Text;
            XmlElement UDPListener = MyXmlDocument.CreateElement("UDPListener");
            UDPListener.InnerText = txtBox_UDPListener.Text;
            XmlElement UDPSender = MyXmlDocument.CreateElement("UDPSender");
            UDPSender.InnerText = txtBox_UDPSender.Text;
            XmlElement Used = MyXmlDocument.CreateElement("Used");
            Used.InnerText = txtBox_Used.Text;

            ParentElement.AppendChild(Name);
            ParentElement.AppendChild(Path);
            ParentElement.AppendChild(Remote);
            ParentElement.AppendChild(TCPListener);
            ParentElement.AppendChild(TCPSender);
            ParentElement.AppendChild(UDPListener);
            ParentElement.AppendChild(UDPSender);
            ParentElement.AppendChild(Used);


            MyXmlDocument.DocumentElement.AppendChild(ParentElement);
            MyXmlDocument.Save(Server.MapPath("~/DataConfig/AppsConfig.xml"));

            Get_Xml();
        }

        protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int id = GridView1.Rows[e.RowIndex].DataItemIndex;

            TextBox txtBox_Name = GridView1.Rows[e.RowIndex].FindControl("TxtEditName") as TextBox;
            TextBox txtBox_Path = GridView1.Rows[e.RowIndex].FindControl("TxtEditName") as TextBox;
            TextBox txtBox_Remote = GridView1.Rows[e.RowIndex].FindControl("TxtEditRemote") as TextBox;
            TextBox txtBox_TCPListener = GridView1.Rows[e.RowIndex].FindControl("TxtEditTCPListener") as TextBox;
            TextBox txtBox_TCPSender = GridView1.Rows[e.RowIndex].FindControl("TxtEditTCPSender") as TextBox;
            TextBox txtBox_UDPListener = GridView1.Rows[e.RowIndex].FindControl("TxtEditUDPListener") as TextBox;
            TextBox txtBox_UDPSender = GridView1.Rows[e.RowIndex].FindControl("TxtEditUDPSender") as TextBox;
            TextBox txtBox_Used = GridView1.Rows[e.RowIndex].FindControl("TxtEditUsed") as TextBox;

            GridView1.EditIndex = -1;

            Get_Xml();

            DataSet ds = GridView1.DataSource as DataSet;
            ds.Tables[0].Rows[id]["Name"] = txtBox_Name.Text;
            ds.Tables[0].Rows[id]["Path"] = txtBox_Path.Text;
            ds.Tables[0].Rows[id]["Remote"] = txtBox_Remote.Text;
            ds.Tables[0].Rows[id]["TCPListener"] = txtBox_TCPListener.Text;
            ds.Tables[0].Rows[id]["TCPSender"] = txtBox_TCPSender.Text;
            ds.Tables[0].Rows[id]["UDPListener"] = txtBox_UDPListener.Text;
            ds.Tables[0].Rows[id]["UDPSender"] = txtBox_UDPSender.Text;
            ds.Tables[0].Rows[id]["Used"] = txtBox_Used.Text;



            ds.WriteXml(Server.MapPath("~/DataConfig/AppsConfig.xml"));

            Get_Xml();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            appsFile = Server.MapPath("~/DataConfig/AppsConfig.xml");
            controller = new Classes.Controller(appsFile);
            Response.Redirect("gettingReady.aspx");

        }
    }
}