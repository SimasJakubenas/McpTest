@openapi.openedge.export FILE(type="REST", executionMode="singleton", useReturnValue="false", writeDataSetBeforeImage="false").
 
 /*------------------------------------------------------------------------
    File        : CustomerService
    Purpose     : REST service to retrieve customer data
    Syntax      : 
    Description : Uses OUTPUT parameters for PASOE REST compatibility
    Author(s)   : sjakubenas
    Created     : Sun Dec 07 20:23:46 EET 2025
    Notes       : 
  ----------------------------------------------------------------------*/

USING Progress.Lang.*.
USING Progress.Json.ObjectModel.JsonArray.
USING Progress.Json.ObjectModel.JsonObject.
USING Progress.Json.ObjectModel.ObjectModelParser.

BLOCK-LEVEL ON ERROR UNDO, THROW.

CLASS CustomerService:
  
    /*------------------------------------------------------------------------------
     Purpose: Get all customers
     Notes: GET /rest/CustomerService/GetCustomers
    ------------------------------------------------------------------------------*/
    @openapi.openedge.export(type="REST", useReturnValue="false", writeDataSetBeforeImage="false").
    METHOD PUBLIC VOID GetCustomers(OUTPUT customers AS LONGCHAR):

        DEFINE VARIABLE oArray AS JsonArray  NO-UNDO.
        DEFINE VARIABLE oCust  AS JsonObject NO-UNDO.

        oArray = NEW JsonArray().

        FOR EACH Customer NO-LOCK:
            oCust = NEW JsonObject().
            oCust:Add("CustNum",  Customer.CustNum).
            oCust:Add("Name",     Customer.Name).
            oCust:Add("Country",  Customer.Country).
            oCust:Add("Balance",  Customer.Balance).

            oArray:Add(oCust).
        END.

        customers = oArray:GetJsonText().

    END METHOD.

    
    /*------------------------------------------------------------------------------
     Purpose: Get a customer by number
     Notes: GET /rest/CustomerService/GetCustomer/123
    ------------------------------------------------------------------------------*/
    @openapi.openedge.export(type="REST", useReturnValue="false", writeDataSetBeforeImage="false").
    METHOD PUBLIC VOID GetCustomer(INPUT custNum AS INTEGER, OUTPUT customer AS LONGCHAR):

        DEFINE VARIABLE oJson AS JsonObject NO-UNDO.
    
        FIND Customer WHERE Customer.CustNum = custNum NO-LOCK NO-ERROR.
    
        oJson = NEW JsonObject().
    
        IF AVAILABLE Customer THEN DO:
            oJson:Add("CustNum",      Customer.CustNum).
            oJson:Add("Name",         Customer.Name).
            oJson:Add("Address",      Customer.Address).
            oJson:Add("Address2",     Customer.Address2).
            oJson:Add("City",         Customer.City).
            oJson:Add("State",        Customer.State).
            oJson:Add("Country",      Customer.Country).
            oJson:Add("Phone",        Customer.Phone).
            oJson:Add("CreditLimit",  Customer.CreditLimit).
            oJson:Add("Balance",      Customer.Balance).
            oJson:Add("EmailAddress", Customer.EmailAddress).
        END.
        ELSE DO:
            oJson:Add("error", "Customer " + STRING(custNum) + " not found").
        END.
    
        customer = oJson:GetJsonText().
    
    END METHOD.

    /*------------------------------------------------------------------------------
     Purpose: Create a new customer
     Notes: POST /rest/CustomerService/CreateCustomer
     Request body: JSON with customer fields
    ------------------------------------------------------------------------------*/
    @openapi.openedge.export(type="REST", useReturnValue="false", writeDataSetBeforeImage="false").
    METHOD PUBLIC VOID CreateCustomer(INPUT customerData AS LONGCHAR, OUTPUT result AS LONGCHAR):

        DEFINE VARIABLE oInput  AS JsonObject NO-UNDO.
        DEFINE VARIABLE oResult AS JsonObject NO-UNDO.
        DEFINE VARIABLE oParser AS ObjectModelParser NO-UNDO.
        DEFINE VARIABLE iNewCustNum AS INTEGER NO-UNDO.
    
        oResult = NEW JsonObject().
    
        /* Parse JSON input */
        oParser = NEW ObjectModelParser().
        oInput = CAST(oParser:Parse(STRING(customerData)), JsonObject).
    
        FIND LAST Customer NO-LOCK NO-ERROR.
        IF AVAILABLE Customer THEN
            iNewCustNum = Customer.CustNum + 1.
        ELSE
            iNewCustNum = 1.
    
        DO TRANSACTION ON ERROR UNDO, THROW:
            CREATE Customer.
            ASSIGN
                Customer.CustNum        = iNewCustNum
                Customer.Name           = (IF oInput:Has("Name") THEN oInput:GetCharacter("Name") ELSE "")
                Customer.Address        = (IF oInput:Has("Address") THEN oInput:GetCharacter("Address") ELSE "")
                Customer.Address2       = (IF oInput:Has("Address2") THEN oInput:GetCharacter("Address2") ELSE "")
                Customer.City           = (IF oInput:Has("City") THEN oInput:GetCharacter("City") ELSE "")
                Customer.State          = (IF oInput:Has("State") THEN oInput:GetCharacter("State") ELSE "")
                Customer.Country        = (IF oInput:Has("Country") THEN oInput:GetCharacter("Country") ELSE "")
                Customer.Phone          = (IF oInput:Has("Phone") THEN oInput:GetCharacter("Phone") ELSE "")
                Customer.CreditLimit    = (IF oInput:Has("CreditLimit") THEN oInput:GetDecimal("CreditLimit") ELSE 0)
                Customer.Balance        = 0 /* New customers start with 0 balance */
                Customer.EmailAddress   = (IF oInput:Has("EmailAddress") THEN oInput:GetCharacter("EmailAddress") ELSE "").
        
            oResult:Add("success", TRUE).
            oResult:Add("message", "Customer created successfully").
            oResult:Add("CustNum", iNewCustNum).
    
            CATCH eError AS Progress.Lang.Error:
                oResult:Add("success", FALSE).
                oResult:Add("error", eError:GetMessage(1)).
            END CATCH.
        END.
    
        result = oResult:GetJsonText().
    
    END METHOD.

    /*------------------------------------------------------------------------------
     Purpose: Update an existing customer
     Notes: PUT /rest/CustomerService/UpdateCustomer/123
            Request body: JSON with fields to update
     Example:
     {
       "Name": "Updated Name",
       "Phone": "555-9999",
       "EmailAddress": "newemail@example.com"
     }
    ------------------------------------------------------------------------------*/
    @openapi.openedge.export(type="REST", useReturnValue="false", writeDataSetBeforeImage="false").
    METHOD PUBLIC VOID UpdateCustomer(
        INPUT custNum AS INTEGER,
        INPUT customerData AS LONGCHAR,
        OUTPUT result AS LONGCHAR):

        DEFINE VARIABLE oInput  AS JsonObject NO-UNDO.
        DEFINE VARIABLE oResult AS JsonObject NO-UNDO.
        DEFINE VARIABLE oParser AS ObjectModelParser NO-UNDO.
    
        oResult = NEW JsonObject().
    
        /* Parse JSON input */
        oParser = NEW ObjectModelParser().
        oInput = CAST(oParser:Parse(STRING(customerData)), JsonObject).
    
        DO TRANSACTION ON ERROR UNDO, THROW:
            FIND Customer WHERE Customer.CustNum = custNum EXCLUSIVE-LOCK NO-ERROR.
        
            IF NOT AVAILABLE Customer THEN DO:
                oResult:Add("success", FALSE).
                oResult:Add("error", "Customer " + STRING(custNum) + " not found").
            END.
            ELSE DO:
                IF oInput:Has("Name") THEN
                    Customer.Name = oInput:GetCharacter("Name").
                IF oInput:Has("Address") THEN
                    Customer.Address = oInput:GetCharacter("Address").
                IF oInput:Has("Address2") THEN
                    Customer.Address2 = oInput:GetCharacter("Address2").
                IF oInput:Has("City") THEN
                    Customer.City = oInput:GetCharacter("City").
                IF oInput:Has("State") THEN
                    Customer.State = oInput:GetCharacter("State").
                IF oInput:Has("Country") THEN
                    Customer.Country = oInput:GetCharacter("Country").
                IF oInput:Has("Phone") THEN
                    Customer.Phone = oInput:GetCharacter("Phone").
                IF oInput:Has("CreditLimit") THEN
                    Customer.CreditLimit = oInput:GetDecimal("CreditLimit").
                IF oInput:Has("Balance") THEN
                    Customer.Balance = oInput:GetDecimal("Balance").
                IF oInput:Has("EmailAddress") THEN
                    Customer.EmailAddress = oInput:GetCharacter("EmailAddress").
            
                oResult:Add("success", TRUE).
                oResult:Add("message", "Customer " + STRING(custNum) + " updated successfully").
            END.
    
            CATCH eError AS Progress.Lang.Error:
                oResult:Add("success", FALSE).
                oResult:Add("error", eError:GetMessage(1)).
            END CATCH.
        END.
    
        result = oResult:GetJsonText().
    
    END METHOD.

    /*------------------------------------------------------------------------------
     Purpose: Delete a customer
     Notes: DELETE /rest/CustomerService/DeleteCustomer/123
    ------------------------------------------------------------------------------*/
    @openapi.openedge.export(type="REST", useReturnValue="false", writeDataSetBeforeImage="false").
    METHOD PUBLIC VOID DeleteCustomer(INPUT custNum AS INTEGER, OUTPUT result AS LONGCHAR):

        DEFINE VARIABLE oResult AS JsonObject NO-UNDO.
    
        oResult = NEW JsonObject().
    
        DO TRANSACTION ON ERROR UNDO, THROW:
            FIND Customer WHERE Customer.CustNum = custNum EXCLUSIVE-LOCK NO-ERROR.
        
            IF NOT AVAILABLE Customer THEN DO:
                oResult:Add("success", FALSE).
                oResult:Add("error", "Customer " + STRING(custNum) + " not found").
            END.
            ELSE DO:
                DELETE Customer.
                oResult:Add("success", TRUE).
                oResult:Add("message", "Customer " + STRING(custNum) + " deleted successfully").
            END.
    
            CATCH eError AS Progress.Lang.Error:
                oResult:Add("success", FALSE).
                oResult:Add("error", eError:GetMessage(1)).
            END CATCH.
        END.
    
        result = oResult:GetJsonText().
    
    END METHOD.

END CLASS.