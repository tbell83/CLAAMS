﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections;

namespace CD6 {
    public partial class sos_view : System.Web.UI.Page {
        SignOutSheet mySOS = new SignOutSheet();
        int sosID;
        DateTime dayCounter=DateTime.Now;
        protected void Page_Load(object sender, EventArgs e)
        {
            lblModifySOSDirections.Visible = true;
            lblModifySOSDirections.Text = "If selected SOS to modify is Permanent, all fields all read only and can not be editted. If term is Non-Permanent, you may only edit the due date for the SOS. Use history drop-down to view all previous changes made to the particular SOS";
            if (!IsPostBack)
            {
                sosID = -1;
                loadOriginal();
            }
        }

        protected void loadOriginal() {
            try {
                sosID = (int)Session["SOSID"];
                //Session.Remove("SOSID");
            } catch {
                Response.Redirect("./sos_search.aspx");
            }

            Dictionary<string, string> names = SignOutSheet.getSosName(sosID);

            mySOS = SignOutSheet.getSOSbyID(sosID);
            //txtRecipient.Text = mySOS.arID.ToString();
            //txtAssigner.Text = mySOS.cladID.ToString();
            txtAssigner.Text = names["Assigner Name"];
            txtRecipient.Text = names["Recipient Name"];
            calIssueDate.SelectedDate = mySOS.dateCreated;

            if (mySOS.assingmentPeriod == 0)  {
                calDue.Visible = true;
                string dueDate = mySOS.dateDue.ToShortDateString();
                DateTime DateDue = Convert.ToDateTime(dueDate);
                calDueDate.SelectedDate = DateDue;
                txtTerm.Text = "Non-Permanent";
            } else {
                txtTerm.Text = "Permanent";
            }

            getAssets(sosID);
            fillHistory(sosID);        
        }

        protected void btnClose_Click(object sender, EventArgs e) {
            Session.Remove("SOSID");
            Response.Redirect("./sos_search.aspx");
        }

        protected void fillHistory(int sosID) {
            ArrayList histories = SosHistory.getHistoryForSOS(sosID);
            ddlHistory.Items.Clear();
            ddlHistory.Items.Add(new ListItem(""));
            ddlHistory.DataSource = histories;
            ddlHistory.DataTextField = "DateCreated";
            ddlHistory.DataValueField = "sosHistoryID";
            ddlHistory.DataBind();
        }

        protected void getAssets(int sosID) {
            ArrayList assets = SignOutSheet.getAssetsForSOS(sosID);
            lbAssets.DataSource = assets;
            lbAssets.DataTextField = "Name";
            lbAssets.DataValueField = "assetID";
            lbAssets.DataBind();
        }

        protected void ddlHistory_SelectedIndexChanged(object sender, EventArgs e) {
            int sosHistoryID = -1;

            try {
                sosHistoryID = Convert.ToInt32(ddlHistory.SelectedValue.ToString());
            } catch {
                loadOriginal();
                return;
            }

            SosHistory history = SosHistory.getHistoryByID(sosHistoryID);

            sosID = history.sosID;
            Session["sosID"] = sosID;
            txtAssigner.Text = history.arID.ToString();
            txtRecipient.Text = history.cladID.ToString();
            txtTerm.Text = history.assingmentPeriod.ToString();
            calIssueDate.SelectedDate = history.dateModified;
            if (history.assingmentPeriod == 0){
                calDue.Visible = true;
                calDueDate.SelectedDate = history.dateDue;
               
            }
        }

        protected void btnSubmitModification_Click(object sender, EventArgs e) 
        {
            string editorID = (string)Session["user"];
            int sosIDDueDate = (int)Session["SOSID"];
           // DataSet ds = Tools.DBAccess.DBCall( "select assetTemplateID, Name from Asset_Template");
            DateTime dueDate = calDueDate.SelectedDate;
            string dialog_header = "" ;
            string dialog_body = "";
           
                    dialog_header = "Modify SOS";
                    dialog_body = ("Are you sure you would like to modify this Sign out Sheet?");
             
            modal1(dialog_header, dialog_body);
           // Response.Redirect("./sos_view.aspx");
        }

        protected void modal(string title, string body) 
        {
            this.Master.modal_header = title;
            this.Master.modal_body = body;
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop", "openModal();", true);
        }

        protected void modal1(string title, string body)
        {
            lblmodifySOSLabel_header.Text = title;
            lblModifySOSModal_body.Text = body;
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop", "modifySOS();", true);
        }

        protected void linkUpload_Click(object sender, EventArgs e) {
            Response.Redirect("./sos_upload.aspx");
        }

        protected void linkPrint_Click(object sender, EventArgs e) {
            Session["sosIdForPrint"] = Session["SOSID"];
            Response.Write("<script>window.open('./sos_print.aspx','_blank');</script>");
        }

        protected void btnModifySOSModalYes_Click(object sender, EventArgs e)
        {
            string dialog_header = "";
            string dialog_body = "";
            string editorID;
            editorID = Session["User"].ToString();
            int sosIDDueDate = (int)Session["SOSID"];
            DateTime dueDate = calDueDate.SelectedDate;

            if (SoSFunctions.UpdateSosHistory(sosIDDueDate, editorID))
            {
                if (SoSFunctions.UpdateSoSDueDate(sosIDDueDate, editorID, dueDate))
                {
                    dialog_header = "SoS Modified";
                    dialog_body = string.Format("SOS {0} has been modified successfully", sosIDDueDate);
                }
                else
                {
                    //CODE TO REMOVE NEW SOSHISTORY RECORD
                    //ERROR DIALOG
                }
            }
            else
            {
                dialog_header = "Error: Modify Failed";
                dialog_body = "Unable to modify record. Please try again.";
            }
            modal(dialog_header, dialog_body);
            btnClose_Click(this, e);
            //Response.Redirect("sos_search.aspx")      //NEED TO REDIRECT TO SEARCH
        }

        protected void btnModifySOSModalNo_Click(object sender, EventArgs e)
        {
            btnSubmitModification_Click(this, e);
        }
    }
}