Imports Newtonsoft.Json

Public Class UserContext

    <JsonProperty("ref")>
    Public Property Ref As String

    <JsonProperty("licenceId")>
    Public Property licenceId As Integer?      ' <-- accepte null

    <JsonProperty("firstName")>
    Public Property FirstName As String

    <JsonProperty("lastName")>
    Public Property LastName As String

    <JsonProperty("phone")>
    Public Property Phone As String

    <JsonProperty("mobilePhone")>
    Public Property MobilePhone As String

    <JsonProperty("email")>
    Public Property Email As String

    <JsonProperty("birthDate")>
    Public Property BirthDate As String

    <JsonProperty("ssoRef")>
    Public Property SsoRef As String

    <JsonProperty("creationDate")>
    Public Property CreationDate As String

    <JsonProperty("updateDate")>
    Public Property UpdateDate As String

    <JsonProperty("nameSearch")>
    Public Property NameSearch As String

    <JsonProperty("accessRightsTemplateName")>
    Public Property AccessRightsTemplateName As String

    <JsonProperty("accessRightsTemplateOrganizationCode")>
    Public Property AccessRightsTemplateOrganizationCode As String

    <JsonProperty("accessRightsTemplateOrganizationName")>
    Public Property AccessRightsTemplateOrganizationName As String

    <JsonProperty("accessRightsTemplateOrganizationDiscr")>
    Public Property AccessRightsTemplateOrganizationDiscr As String
End Class

