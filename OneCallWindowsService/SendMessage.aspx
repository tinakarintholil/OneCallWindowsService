<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SendMessage.aspx.cs" Inherits="OneCallWindowsService.SendMessage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
    
<head runat="server">
    
    
    <meta charset="utf-8" />
    <title>SendMessage One Call</title>
    <meta name="viewport" content="width=device-width initial-scale=1.0" />

    <script src="Scripts/jquery-1.12.4.min.js"></script>
    <script src="Scripts/jquery-ui-1.12.1.min.js"></script>
    <link href="Content/themes/base/datepicker.css" rel="stylesheet" />
    <link href="Content/themes/base/jquery-ui.min.css" rel="stylesheet" />
    <link href="StyleSheet1.css" rel="stylesheet" />
  
<%--    -<!-- CSS -->
    <link rel="stylesheet" type="text/css" href="//cdnjs.cloudflare.com/ajax/libs/chosen/1.1.0/chosen.min.css"/>

<!-- JS -->
<script type="text/javascript" src="//cdnjs.cloudflare.com/ajax/libs/chosen/1.1.0/chosen.jquery.min.js"></script>--%>
 
    <style type="text/css">
        .ui-datepicker {
           background: #333;
           border: 1px solid #555;
           color: #EEE;
         
            }

         table { border-spacing: 0px; } /* cellspacing */
         th, td { padding: 5px;}

         a:link {
         text-decoration: none;
         color:red!important;
           }

          a:visited {
              text-decoration: none;
              color:red !important;
              }

a:hover {
    text-decoration: underline;
    color:red !important;
    background-color:white !important;
}
  
       

 
  
        </style>
    <script type="text/javascript">


        $(document).ready(function ()
        {
                      
            $("#txtRouteDate").datepicker({
                minDate: "+1d",
                showAnim: "slide"
                 }).datepicker("setDate", "+1");

  

        });

       

       

        function validate()
        {
           
            
          
           
            var errs = ''; var datselected = false; var branchselected = false; var ret = false;
           
      
            var x = document.getElementById("ddl");
            for (var i = 1; i < x.options.length; i++) {
                if (x.options[i].selected == true)  {
                    branchselected = true;
                    break;
                }
            }
            if (!branchselected)
                errs += "Please Select Branch To Upload\r\n";
            var y = document.getElementById("txtRouteDate").value;
         
            if (y == "") {
              
                errs += "Please Select RouteDate To Upload\r\n";
               
            }
           
            
          
            
            if (errs.length > 0) {
                alert(errs);
                
                document.getElementById("lblMessage").innerText = '';
                return false;
                event.preventDefault;
               
               
            }
            else
                
                return true;
           
            
           
        }

        function validateResponse()
        {
            

        

            var errs = ''; var groupselected = false; var branchselected = false; var ret = false; var check = false;

            
            var x = document.getElementById("ddlResponse");
            document.getElementById("lblResponseMessage").innerHTML = "";
         

          
            for (var i = 1; i < x.options.length; i++) {
                if (x.options[i].selected == true) {
                    branchselected = true;
                    break;
                }
            }
            if (!branchselected)
                errs += "Please Select Branch To Get Response\r\n";


            var radios = document.getElementsByName("group");
         
            for (var i = 0, len = radios.length; i < len; i++) {
                if (radios[i].checked) {
                    groupselected=true;
                    break
                }
            }

            if (!groupselected) {

                errs += "Please Select At Least One Group \r\n";

            }

            



            if (errs.length > 0) {
                alert(errs);
                document.getElementById("btnReturn").value = "Get Response From One Call";
                document.getElementById("lblMessage").innerText = '';
                return false;
                event.preventDefault;


            }
            else
                document.getElementById("lblMessage").innerText = '';
                return true;



        }




      
        function alertMe()
        {
            alert('Please enter values in Route and Branch ');
            return false;
        }


        function goBack()
        {
            window.history.back();
        }

       
       
      
       
    </script>
</head>

<body>
    <form id="form1" runat="server">
 

        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server">
               </asp:ScriptManager>
           
              <%--<div id="divUserLog"  style="color: white; width: 200px;float:right" runat="server">
                <section id="login">                   
                     <asp:Label ID="lblWelcome" CssClass="username" runat="server" Text="" ForeColor="Black"></asp:Label>
                </section>
             </div>  
           <div style="clear: both;"></div>--%>
           

           <p style="text-align:center; color:#ffffff;padding:0px;background-color: #0D5692;margin:0px">One Call Upload<%--<img src="Images/onecall.jpg" style="height:28px;width:104px;vertical-align:middle" />--%> </p>

            <div style="width: 30%; margin:0 auto;padding:2px;">
                    <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                    <ContentTemplate>
                <table style="width:100% ;">
                                        <tr>
                        <td style="text-align: right">Route Date:</td>
                        <td style="text-align: left">
                            <asp:TextBox ID="txtRouteDate"    runat="server" onkeydown="return (event.keyCode != 13);return false;"></asp:TextBox>

                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: right" >Branch:</td>
                        <td style="text-align: left" >
                            

                                                    <asp:DropDownList ID="ddl" CssClass="drop" runat="server" Width="188px" >
                           
                            </asp:DropDownList>
                           

                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: center"></td>
                        <td style="text-align: left">
                            <asp:Button runat="server" ID="btnSubmit" CssClass="btsubmit" OnClientClick="return validate();" Text="Submit" OnClick="btnSubmit_Click" />&nbsp;&nbsp;&nbsp;&nbsp;
                     

                             
                           
                    &nbsp;&nbsp;
                                                       
                    </tr>
                    <tr>
                        <td style="text-align: center"></td>
                        <td style="text-align: left;">
                            <asp:Label ID="lblMessage" CssClass="message-error" Text="" runat="server" ForeColor="Red"></asp:Label></td>
                    </tr>

                                 

                </table>

                         </ContentTemplate>
        </asp:UpdatePanel>
                   <asp:UpdateProgress ID="UpdateProgress2" AssociatedUpdatePanelID="UpdatePanel2" runat="server">
            <ProgressTemplate>
                <div id="topDiv" style="display:inline-block;text-align:center;margin-left:130px;width:140px">
                      Processing...<span><img src="Images/animatedload.gif" alt="Loading..."></span>
                </div>
              
            
            </ProgressTemplate>
        </asp:UpdateProgress>

                <asp:HiddenField runat="server" ID="hdBranch" Value="" />

            </div>
            <a style="margin-left:30px;text-decoration:none;" href="javascript:goBack();"><i style="font-size:small">Back</i></a>

             
             <p style="text-align: center; color:#ffffff;padding:0px; background-color: #0D5692;margin:0px;">One Call Response &nbsp;&nbsp;<%--<img src="Images/onecall.jpg" style="height:28px;width:104px;vertical-align:middle" />--%> </p>
            <div style="width: 20%; margin:0 auto;padding:2px;">
                
                  <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>

                 <table style="width:100% ;">
                     <tr><td style="text-align: right" >Group1:</td>
                         <td style="text-align:left" ><asp:RadioButton ID="RadioButton1"  GroupName="group" runat="server"  /></td>
                         </tr>

                     <tr><td style="text-align: right">Group2:</td>
                         <td style="text-align:left"><asp:RadioButton ID="RadioButton2"  GroupName="group" runat="server"  /></td></tr>

                     <tr><td style="text-align: right">Group3:</td>
                         <td style="text-align:left"><asp:RadioButton ID="RadioButton3" GroupName="group" runat="server"  /></td></tr>

                     <tr><td style="text-align: right">Group4:</td>
                         <td style="text-align:left"><asp:RadioButton ID="RadioButton4"  GroupName="group" runat="server"  /></td></tr>

                     <tr><td style="text-align: right">Group5:</td>
                         <td style="text-align:left"><asp:RadioButton ID="RadioButton5"  GroupName="group" runat="server"  /></td></tr>

                     <tr><td style="text-align: right">Group6:</td>
                         <td style="text-align:left"><asp:RadioButton ID="RadioButton6" name="groups" GroupName="group" runat="server"  /></td></tr>
                    
                     <tr><td style="text-align: right">Group7:</td>
                         <td style="text-align:left"><asp:RadioButton ID="RadioButton7" name="groups" GroupName="group" runat="server"  /></td></tr>
                   
                     <tr><td style="text-align: right">Group8:</td>
                         <td style="text-align:left"><asp:RadioButton ID="RadioButton8" name="groups" GroupName="group" runat="server"  /></td></tr>
                    
                     <tr><td style="text-align: right">Branch:</td>
                         <td style="text-align:left"> <asp:DropDownList ID="ddlResponse" runat="server" Width="188px">
                           
                            </asp:DropDownList></td></tr>
                    


                     <tr><td style="text-align: center" colspan="2"><asp:Button id="btnReturn"  CssClass="auto-style2" Text="Get Response From One call" runat="server" OnClientClick="return validateResponse();" OnClick="btnReturn_Click" Width="202px" />   </td>
                         </tr>
                       <tr>
                        
                        <td style="text-align: center" colspan="2">
                            <asp:Label ID="lblResponseMessage" CssClass="message-error" Text="" runat="server" ForeColor="Red"></asp:Label></td>
                    </tr>


                     </table>
                 </ContentTemplate>
        </asp:UpdatePanel>
                 <asp:UpdateProgress ID="UpdateProgress1" AssociatedUpdatePanelID="UpdatePanel1" runat="server">
            <ProgressTemplate>
                <div id="bottomdiv" style="display:inline-block;text-align:center;margin-left:50px;">
                      Processing...<span><img src="Images/animatedload.gif" alt="Loading..." /></span>
                </div>
              
            
            </ProgressTemplate>
        </asp:UpdateProgress>
              
                
              </div>





        </div>




        <br />
        
     
    </form>
</body>
</html>
