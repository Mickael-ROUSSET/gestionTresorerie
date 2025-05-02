''https://learn.microsoft.com/fr-fr/entra/identity/role-based-access-control/custom-create
''Ajout du Namespace System.DirectoryServices
'Imports System.DirectoryServices

'Module GereActiveDirectory

'    Public Sub ConnexionLdap()
'        Try
'            Dim Ldap As New DirectoryEntry("LDAP://votre-nom-AD", "Login", "Password")
'        Catch Ex As Exception
'            MessageBox.Show(Ex.Message)
'        End Try
'    End Sub
'    Public Sub rechercheLdap()
'        Dim Ldap As New DirectoryEntry("LDAP://votre-nom-AD", "Login", "Password")

'        Dim searcher As DirectorySearcher = New DirectorySearcher(Ldap)
'        'Filtrage sur les utilisateursSélectionnez
'        searcher.Filter = "(objectClass=user)"
'        'On boucle pour récupérer et afficher les informations désiréesSélectionnez
'        Dim DirEntry As DirectoryEntry

'        For Each result As SearchResult In searcher.FindAll
'            'On récupère l'entrée trouvée lors de la recherche
'            DirEntry = result.GetDirectoryEntry

'            'On peut maintenant afficher les informations désirées
'            MsgBox("Login : " + DirEntry.Properties("SAMAccountName").Value)
'            MsgBox("Nom : " + DirEntry.Properties("sn").Value)
'            MsgBox("Prénom : " + DirEntry.Properties("givenName").Value)
'            MsgBox("Email : " + DirEntry.Properties("mail").Value)
'            MsgBox("Tél : " + DirEntry.Properties("TelephoneNumber").Value)
'            MsgBox("Description : " + DirEntry.Properties("description").Value)
'            MsgBox("-------------------")
'        Next
'    End Sub

'    Public Sub modifierLdap()
'        ' Connexion à l'annuaire

'        Dim Ldap As New DirectoryEntry("LDAP:'votre-nom-AD", "Login", "Password")
'        ' Nouvel objet pour instancier la recherche
'        Dim searcher As DirectorySearcher = New DirectorySearcher(Ldap)
'        ' On modifie le filtre pour ne chercher que l'user dont le nom de login est TEST
'        searcher.Filter = "(SAMAccountName=TEST)"
'        ' Pas de boucle foreach car on ne cherche qu'un user
'        Dim result As SearchResult = searcher.FindOne()
'        ' On récupère l'objet trouvé lors de la recherche
'        Dim DirEntry As DirectoryEntry = result.GetDirectoryEntry()
'        ' On modifie la propriété description de l'utilisateur TEST
'        DirEntry.Properties("description").Value = "Nouvelle description pour TEST"
'        ' Et son numéro de téléphone
'        DirEntry.Properties("TelephoneNumber").Value = "0123456789"
'        ' On envoie les changements à Active Directory
'        DirEntry.CommitChanges()
'    End Sub
'    Public Sub ajouterUtilisateur()
'        ' Connexion à l'annuaire

'        Dim Ldap As DirectoryEntry = New DirectoryEntry("LDAP://votre-nom-AD", "Login", "Password")
'        ' Création du user Test User et initialisation de ses propriétés
'        Dim user As DirectoryEntry = Ldap.Children.Add("cn=Test User", "user")
'        user.Properties("SAMAccountName").Add("testuser")
'        user.Properties("sn").Add("User")
'        user.Properties("givenName").Add("Test")
'        user.Properties("description").Add("Compte de test créé par le code")
'        ' On envoie les modifications au serveur
'        user.CommitChanges()


'        ' On va maintenant lui définir son password. L'utilisateur doit avoir été créé
'        ' et sauvé avant de pouvoir faire cette étape
'        user.Invoke("SetPassword", New Object() {"motdepasse"})
'        ' On va maintenant activer le compte  ADS_UF_NORMAL_ACCOUNT
'        user.Properties("userAccountControl").Value = "0x0200"
'        ' On envoie les modifications au serveur
'        user.CommitChanges()
'    End Sub
'End Module
