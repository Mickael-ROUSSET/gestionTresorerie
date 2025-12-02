Imports Newtonsoft.Json

Public Class LicenceDetail

    <JsonProperty("licenceId")>
    Public Property LicenceId As String

    <JsonProperty("downloadIdentifier")>
    Public Property DownloadIdentifier As String

    <JsonProperty("taxDeductionDownloadIdentifier")>
    Public Property TaxDeductionDownloadIdentifier As String

    <JsonProperty("management")>
    Public Property Management As Boolean

    <JsonProperty("paidByThirdParty")>
    Public Property PaidByThirdParty As Boolean

    <JsonProperty("ref")>
    Public Property Ref As String

    <JsonProperty("creationDate")>
    Public Property CreationDate As Date?

    <JsonProperty("updateDate")>
    Public Property UpdateDate As Date?

    <JsonProperty("homeAssociation")>
    Public Property HomeAssociation As HomeAssociation

    <JsonProperty("licence")>
    Public Property Licence As LicenceInfo

End Class

Public Class HomeAssociation
    Public Property code As String
    Public Property name As String
End Class

Public Class LicenceInfo
    Public Property ref As String
    Public Property product As LicenceProduct
    Public Property season As LicenceSeason
End Class

Public Class LicenceProduct
    Public Property name As String
End Class

Public Class LicenceSeason
    Public Property title As String
End Class
