﻿#region Copyright (c) 2002-2010, Bas Geertsema, Xih Solutions (http://www.xihsolutions.net), Thiago.Sayao, Pang Wu, Ethem Evlice
/*
Copyright (c) 2002-2010, Bas Geertsema, Xih Solutions
(http://www.xihsolutions.net), Thiago.Sayao, Pang Wu, Ethem Evlice.
All rights reserved. http://code.google.com/p/msnp-sharp/

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice,
  this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.
* Neither the names of Bas Geertsema or Xih Solutions nor the names of its
  contributors may be used to endorse or promote products derived from this
  software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 'AS IS'
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion

using System;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace MSNPSharp.IO
{
    using MSNPSharp.MSNWS.MSNABSharingService;
    using MSNPSharp.Core;

    /// <summary>
    /// ContactList file maintainer
    /// </summary>
    [Serializable]
    [XmlRoot("ContactList")]
    public class XMLContactList : MCLSerializer
    {
        [NonSerialized]
        private bool initialized = false;

        [NonSerialized]
        private int requestCircleCount = 0;

        public static XMLContactList LoadFromFile(string filename, MclSerialization st, NSMessageHandler handler, bool useCache)
        {
            return (XMLContactList)LoadFromFile(filename, st, typeof(XMLContactList), handler, useCache);
        }

        /// <summary>
        /// Initialize contacts from mcl file. Creates contacts based on MemberShipList, Groups, CircleResults and AddressbookContacts.
        /// MemberShipList, Groups, CircleResults and AddressbookContacts is pure clean and no contains DELTAS...
        /// So, member.Deleted is not valid here...
        /// </summary>
        private void Initialize()
        {
            if (initialized)
                return;

            initialized = true;

            // Create Memberships
            SerializableDictionary<string, SerializableDictionary<string, BaseMember>> ms =
                SelectTargetMemberships(ServiceFilterType.Messenger);

            if (ms != null)
            {
                foreach (string role in ms.Keys)
                {
                    MSNLists msnlist = NSMessageHandler.ContactService.GetMSNList(role);
                    foreach (BaseMember bm in ms[role].Values)
                    {
                        long cid = 0;
                        string account = null;
                        ClientType type = ClientType.None;

                        if (bm is PassportMember)
                        {
                            type = ClientType.PassportMember;
                            PassportMember pm = (PassportMember)bm;
                            if (!pm.IsPassportNameHidden)
                            {
                                account = pm.PassportName;
                            }
                            cid = Convert.ToInt64(pm.CID);
                        }
                        else if (bm is EmailMember)
                        {
                            type = ClientType.EmailMember;
                            account = ((EmailMember)bm).Email;
                        }
                        else if (bm is PhoneMember)
                        {
                            type = ClientType.PhoneMember;
                            account = ((PhoneMember)bm).PhoneNumber;
                        }

                        if (account != null && type != ClientType.None)
                        {
                            string displayname = bm.DisplayName == null ? account : bm.DisplayName;
                            Contact contact = NSMessageHandler.ContactList.GetContact(account, displayname, type);
                            contact.CID = cid;
                            contact.Lists |= msnlist;
                        }
                    }
                }
            }


            #region Create Groups

            foreach (GroupType group in Groups.Values)
            {
                NSMessageHandler.ContactGroups.AddGroup(new ContactGroup(group.groupInfo.name, group.groupId, NSMessageHandler, group.groupInfo.IsFavorite));
            }

            #endregion

            #region Restore CID contact table

            foreach (string abId in AddressbookContacts.Keys)
            {
                ContactType[] contactList = new ContactType[AddressbookContacts[abId].Count];

                AddressbookContacts[abId].Values.CopyTo(contactList, 0);
                SaveContactTable(contactList);
            }

            #endregion

            #region Restore Circles

            RestoreWLConnections();
            long[] CIDs = FilterWLConnections(new List<long>(WLConnections.Keys), RelationshipState.Accepted);
            RestoreCircles(CIDs, RelationshipState.Accepted);
            CIDs = FilterWLConnections(new List<long>(WLConnections.Keys), RelationshipState.WaitingResponse);
            RestoreCircles(CIDs, RelationshipState.WaitingResponse);

            #endregion

            #region Restore default addressbook
            if (AddressbookContacts.ContainsKey(WebServiceConstants.MessengerIndividualAddressBookId))
            {
                SerializableDictionary<Guid, ContactType> defaultPage = AddressbookContacts[WebServiceConstants.MessengerIndividualAddressBookId];
                foreach (ContactType contactType in defaultPage.Values)
                {
                    ReturnState updateResult = UpdateContact(contactType); //Restore contatcs.
                    if ((updateResult & ReturnState.UpdateError) != ReturnState.None)
                    {
                        Trace.WriteLineIf(Settings.TraceSwitch.TraceError, "[Initialize Error]: update contact error.");
                    }
                }
            }

            #endregion

        }

        private bool IsContactTableEmpty()
        {
            lock (contactTable)
                return contactTable.Count == 0;
        }

        private bool IsPendingCreateConfirmCircle(string abId)
        {
            lock (PendingCreateCircleList)
                return PendingCreateCircleList.ContainsKey(new Guid(abId));
        }

        private bool IsPendingCreateConfirmCircle(Guid abId)
        {
            lock (PendingCreateCircleList)
                return PendingCreateCircleList.ContainsKey(abId);
        }

        #region New MembershipList

        private SerializableDictionary<string, ServiceMembership> mslist = new SerializableDictionary<string, ServiceMembership>(0);
        public SerializableDictionary<string, ServiceMembership> MembershipList
        {
            get
            {
                return mslist;
            }
            set
            {
                mslist = value;
            }
        }

        public string MembershipLastChange
        {
            get
            {
                if (MembershipList.Keys.Count == 0)
                    return WebServiceConstants.ZeroTime;

                List<Service> services = new List<Service>();
                foreach (string sft in MembershipList.Keys)
                    services.Add(new Service(MembershipList[sft].Service));

                services.Sort();
                return services[services.Count - 1].LastChange;
            }
        }

        internal Service SelectTargetService(string type)
        {
            if (MembershipList.ContainsKey(type))
                return MembershipList[type].Service;

            return null;
        }

        internal SerializableDictionary<string, SerializableDictionary<string, BaseMember>> SelectTargetMemberships(string serviceFilterType)
        {
            if (MembershipList.ContainsKey(serviceFilterType))
                return MembershipList[serviceFilterType].Memberships;

            return null;
        }

        public void AddMemberhip(string servicetype, string account, ClientType type, string memberrole, BaseMember member)
        {
            SerializableDictionary<string, SerializableDictionary<string, BaseMember>> ms = SelectTargetMemberships(servicetype);
            if (ms != null)
            {
                if (!ms.ContainsKey(memberrole))
                    ms.Add(memberrole, new SerializableDictionary<string, BaseMember>(0));

                ms[memberrole][Contact.MakeHash(account, type, WebServiceConstants.MessengerIndividualAddressBookId)] = member;
            }
        }

        public void RemoveMemberhip(string servicetype, string account, ClientType type, string memberrole)
        {
            SerializableDictionary<string, SerializableDictionary<string, BaseMember>> ms = SelectTargetMemberships(servicetype);
            if (ms != null)
            {
                string hash = Contact.MakeHash(account, type, WebServiceConstants.MessengerIndividualAddressBookId);
                if (ms.ContainsKey(memberrole) && ms[memberrole].ContainsKey(hash))
                {
                    ms[memberrole].Remove(hash);
                }
            }
        }

        /// <summary>
        /// Try to remove a contact from a specific addressbook.
        /// </summary>
        /// <param name="abId">The specific addressbook identifier.</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns>If the contact exists and removed successfully, return true, else return false.</returns>
        internal bool RemoveContactFromAddressBook(Guid abId, Guid contactId)
        {
            return RemoveContactFromAddressBook(abId.ToString("D"), contactId);
        }

        /// <summary>
        /// Try to remove a contact from a specific addressbook.
        /// </summary>
        /// <param name="abId">The specific addressbook identifier.</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns>If the contact exists and removed successfully, return true, else return false.</returns>
        internal bool RemoveContactFromAddressBook(string abId, Guid contactId)
        {
            string lowerId = abId.ToLowerInvariant();
            lock (AddressbookContacts)
            {
                if (AddressbookContacts.ContainsKey(lowerId))
                {
                    if (AddressbookContacts[lowerId].ContainsKey(contactId))
                    {
                        return AddressbookContacts[lowerId].Remove(contactId);
                    }
                }
            }

            return false;
        }


        private bool RemoveContactFromContactTable(long CID)
        {
            lock (contactTable)
                return contactTable.Remove(CID);
        }

        /// <summary>
        /// Remove an item in AddressBooksInfo property by giving an addressbook Id.
        /// </summary>
        /// <param name="abId"></param>
        /// <returns></returns>
        private bool RemoveAddressBookInfo(string abId)
        {

            lock (AddressBooksInfo)
                return AddressBooksInfo.Remove(abId.ToLowerInvariant());
        }

        /// <summary>
        /// Remove an item from AddressbookContacts property.
        /// </summary>
        /// <param name="abId">The addressbook page of a specified contact page.</param>
        /// <returns></returns>
        private bool RemoveAddressBookContatPage(string abId)
        {
            lock (AddressbookContacts)
            {
                return AddressbookContacts.Remove(abId.ToLowerInvariant());
            }
        }

        /// <summary>
        /// Add or update a contact in the specific address book.
        /// </summary>
        /// <param name="abId">The identifier of addressbook.</param>
        /// <param name="contact">The contact to be added/updated.</param>
        /// <returns>If the contact added to the addressbook, returen true, if the contact is updated (not add), return false.</returns>
        internal bool SetContactToAddressBookContactPage(string abId, ContactType contact)
        {
            string lowerId = abId.ToLowerInvariant();
            bool returnval = false;

            lock (AddressbookContacts)
            {
                if (!AddressbookContacts.ContainsKey(lowerId))
                {
                    AddressbookContacts.Add(lowerId, new SerializableDictionary<Guid, ContactType>());
                    returnval = true;
                }

                AddressbookContacts[lowerId][new Guid(contact.contactId)] = contact;
            }

            return returnval;
        }

        private bool SetAddressBookInfoToABInfoList(string abId, ABFindContactsPagedResultTypeAB abInfo)
        {
            string lowerId = abId.ToLowerInvariant();
            if (AddressBooksInfo == null)
                return false;

            lock (AddressBooksInfo)
                AddressBooksInfo[lowerId] = abInfo;
            return true;
        }


        private bool HasContact(long CID)
        {
            lock (contactTable)
                return contactTable.ContainsKey(CID);
        }

        private bool HasWLConnection(long CID)
        {
            lock (WLConnections)
                return WLConnections.ContainsKey(CID);
        }

        private bool HasWLConnection(string abId)
        {
            lock (WLInverseConnections)
            {
                return WLInverseConnections.ContainsKey(abId);
            }
        }

        /// <summary>
        /// Check whether we've saved the specified addressbook.
        /// </summary>
        /// <param name="abId"></param>
        /// <returns></returns>
        private bool HasAddressBook(string abId)
        {
            string lowerId = abId.ToLowerInvariant();
            if (AddressBooksInfo == null)
                return false;

            lock (AddressBooksInfo)
                return AddressBooksInfo.ContainsKey(lowerId);
        }

        /// <summary>
        /// Check whether the specific contact page exist.
        /// </summary>
        /// <param name="abId">The addressbook identifier of a specific contact page.</param>
        /// <returns></returns>
        private bool HasAddressBookContactPage(string abId)
        {
            string lowerId = abId.ToLowerInvariant();
            if (AddressbookContacts == null)
                return false;

            bool returnValue = false;

            lock (AddressbookContacts)
            {
                if (AddressbookContacts.ContainsKey(lowerId))
                {
                    if (AddressbookContacts[lowerId] != null)
                    {
                        returnValue = true;
                    }
                }
            }

            return returnValue;
        }

        internal bool HasContact(string abId, Guid contactId)
        {
            string lowerId = abId.ToLowerInvariant();
            lock (AddressbookContacts)
            {
                if (!AddressbookContacts.ContainsKey(lowerId))
                    return false;

                return AddressbookContacts[lowerId].ContainsKey(contactId);
            }
        }

        private bool HasMemberhip(string servicetype, string account, ClientType type, string memberrole)
        {
            SerializableDictionary<string, SerializableDictionary<string, BaseMember>> ms = SelectTargetMemberships(servicetype);
            return (ms != null) && ms.ContainsKey(memberrole) && ms[memberrole].ContainsKey(Contact.MakeHash(account, type, WebServiceConstants.MessengerIndividualAddressBookId));
        }

        /// <summary>
        /// Get a basemember from membership list.
        /// </summary>
        /// <param name="servicetype"></param>
        /// <param name="account"></param>
        /// <param name="type"></param>
        /// <param name="memberrole"></param>
        /// <returns>If the member not exist, return null.</returns>
        public BaseMember SelectBaseMember(string servicetype, string account, ClientType type, string memberrole)
        {
            string hash = Contact.MakeHash(account, type, WebServiceConstants.MessengerIndividualAddressBookId);
            SerializableDictionary<string, SerializableDictionary<string, BaseMember>> ms = SelectTargetMemberships(servicetype);
            if ((ms != null) && ms.ContainsKey(memberrole) && ms[memberrole].ContainsKey(hash))
            {
                return ms[memberrole][hash];
            }
            return null;
        }

        /// <summary>
        /// Get a contact from a specific addressbook by providing the addressbook identifier and contact identifier.
        /// </summary>
        /// <param name="abId">The addressbook identifier.</param>
        /// <param name="contactId">The contactidentifier.</param>
        /// <returns>If the contact exist, return the contact object, else return null.</returns>
        internal ContactType SelectContactFromAddressBook(string abId, Guid contactId)
        {
            string lowerId = abId.ToLowerInvariant();
            if (!HasContact(abId, contactId))
                return null;
            return AddressbookContacts[lowerId][contactId];
        }

        public virtual void Add(
            Dictionary<Service,
            Dictionary<string,
            Dictionary<string, BaseMember>>> range)
        {
            foreach (Service svc in range.Keys)
            {
                foreach (string role in range[svc].Keys)
                {
                    foreach (string hash in range[svc][role].Keys)
                    {
                        if (!mslist.ContainsKey(svc.ServiceType))
                            mslist.Add(svc.ServiceType, new ServiceMembership(svc));

                        if (!mslist[svc.ServiceType].Memberships.ContainsKey(role))
                            mslist[svc.ServiceType].Memberships.Add(role, new SerializableDictionary<string, BaseMember>(0));

                        if (mslist[svc.ServiceType].Memberships[role].ContainsKey(hash))
                        {
                            if (/* mslist[svc.ServiceType].Memberships[role][hash].LastChangedSpecified
                                && */
                                WebServiceDateTimeConverter.ConvertToDateTime(mslist[svc.ServiceType].Memberships[role][hash].LastChanged).CompareTo(
                                WebServiceDateTimeConverter.ConvertToDateTime(range[svc][role][hash].LastChanged)) <= 0)
                            {
                                mslist[svc.ServiceType].Memberships[role][hash] = range[svc][role][hash];
                            }
                        }
                        else
                        {
                            mslist[svc.ServiceType].Memberships[role].Add(hash, range[svc][role][hash]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the hidden representative's CID by providing addressbook Id from the inverse connection list.
        /// </summary>
        /// <param name="abId"></param>
        /// <returns></returns>
        private long SelectWLConnection(string abId)
        {
            if (string.IsNullOrEmpty(abId))
                return long.MinValue;

            string lowerId = abId.ToLowerInvariant();
            if (!HasWLConnection(lowerId))
                return long.MinValue;

            lock (WLInverseConnections)
                return WLInverseConnections[lowerId];

        }

        private string SelectWLConnection(long CID)
        {
            if (HasWLConnection(CID))
            {
                lock (WLConnections)
                    return WLConnections[CID];
            }

            return string.Empty;
        }

        private string[] SelectWLConnection(List<long> CIDs, RelationshipState state)
        {
            List<string> abIds = new List<string>(0);

            lock (WLConnections)
            {
                foreach (long CID in CIDs)
                {
                    if (HasWLConnection(CID) && HasContact(CID))
                    {
                        string abId = WLConnections[CID];
                        ContactType contact = SelecteContact(CID);

                        if (state == RelationshipState.None)
                        {
                            abIds.Add(abId);
                        }
                        else
                        {
                            if (GetCircleMemberRelationshipStateFromNetworkInfo(contact.contactInfo.NetworkInfoList) == state)
                                abIds.Add(abId);
                        }
                        
                    }
                }
            }

            return abIds.ToArray();
        }

        private long[] FilterWLConnections(List<long> CIDs, RelationshipState state)
        {
            List<long> returnValues = new List<long>(0);


            foreach (long CID in CIDs)
            {
                if (HasWLConnection(CID) && HasContact(CID))
                {
                    ContactType contact = SelecteContact(CID);

                    if (state == RelationshipState.None)
                    {
                        returnValues.Add(CID);
                    }
                    else
                    {
                        RelationshipState representativeRelationshipState = GetCircleMemberRelationshipStateFromNetworkInfo(contact.contactInfo.NetworkInfoList);
                        if (representativeRelationshipState == state)
                            returnValues.Add(CID);
                    }

                }
            }

            return returnValues.ToArray();
        }

        private CircleInverseInfoType SelectCircleInverseInfo(string abId)
        {
            if (string.IsNullOrEmpty(abId))
                return null;

            abId = abId.ToLowerInvariant();

            lock (CircleResults)
            {
                if (!CircleResults.ContainsKey(abId))
                    return null;
                return CircleResults[abId];
            }
        }

        /// <summary>
        /// Get a hidden representative for a addressbook by CID.
        /// </summary>
        /// <param name="CID"></param>
        /// <returns></returns>
        private ContactType SelecteContact(long CID)
        {
            if (!HasContact(CID))
            {
                return null;
            }

            lock (contactTable)
                return contactTable[CID];
        }

        public XMLContactList Merge(DeltasList deltas)
        {
            Initialize();

            if (deltas.MembershipDeltas.Count > 0)
            {
                foreach (FindMembershipResultType membershipResult in deltas.MembershipDeltas)
                {
                    Merge(membershipResult);
                }
            }

            if (deltas.AddressBookDeltas.Count > 0)
            {
                foreach (ABFindContactsPagedResultType findcontactspagedResult in deltas.AddressBookDeltas)
                {
                    Merge(findcontactspagedResult);
                }

            }

            return this;
        }

        public XMLContactList Merge(FindMembershipResultType findMembership)
        {
            Initialize();

            // Process new FindMemberships (deltas)
            if (null != findMembership && null != findMembership.Services)
            {
                foreach (ServiceType serviceType in findMembership.Services)
                {
                    Service oldService = SelectTargetService(serviceType.Info.Handle.Type);

                    if (oldService == null ||
                        WebServiceDateTimeConverter.ConvertToDateTime(oldService.LastChange)
                        < WebServiceDateTimeConverter.ConvertToDateTime(serviceType.LastChange))
                    {
                        if (serviceType.Deleted)
                        {
                            if (MembershipList.ContainsKey(serviceType.Info.Handle.Type))
                            {
                                MembershipList.Remove(serviceType.Info.Handle.Type);
                            }
                        }
                        else
                        {
                            Service updatedService = new Service();
                            updatedService.Id = int.Parse(serviceType.Info.Handle.Id);
                            updatedService.ServiceType = serviceType.Info.Handle.Type;
                            updatedService.LastChange = serviceType.LastChange;
                            updatedService.ForeignId = serviceType.Info.Handle.ForeignId;

                            if (oldService == null)
                            {
                                MembershipList.Add(updatedService.ServiceType, new ServiceMembership(updatedService));
                            }

                            if (null != serviceType.Memberships)
                            {
                                #region Messenger memberhips

                                if (ServiceFilterType.Messenger == serviceType.Info.Handle.Type)
                                {
                                    foreach (Membership membership in serviceType.Memberships)
                                    {
                                        if (null != membership.Members)
                                        {
                                            string memberrole = membership.MemberRole;
                                            List<BaseMember> members = new List<BaseMember>(membership.Members);
                                            members.Sort(CompareBaseMembers);

                                            foreach (BaseMember bm in members)
                                            {
                                                long cid = 0;
                                                string account = null;
                                                ClientType type = ClientType.None;

                                                if (bm is PassportMember)
                                                {
                                                    type = ClientType.PassportMember;
                                                    PassportMember pm = bm as PassportMember;
                                                    if (!pm.IsPassportNameHidden)
                                                    {
                                                        account = pm.PassportName;
                                                    }
                                                    cid = Convert.ToInt64(pm.CID);
                                                }
                                                else if (bm is EmailMember)
                                                {
                                                    type = ClientType.EmailMember;
                                                    account = ((EmailMember)bm).Email;
                                                }
                                                else if (bm is PhoneMember)
                                                {
                                                    type = ClientType.PhoneMember;
                                                    account = ((PhoneMember)bm).PhoneNumber;
                                                }
                                                else if (bm is CircleMember)
                                                {
                                                    type = ClientType.CircleMember;
                                                    account = ((CircleMember)bm).CircleId;
                                                    if (!circlesMembership.ContainsKey(memberrole))
                                                    {
                                                        circlesMembership.Add(memberrole, new List<CircleMember>(0));
                                                    }
                                                    circlesMembership[memberrole].Add(bm as CircleMember);
                                                    Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, serviceType.Info.Handle.Type + " Membership " + bm.GetType().ToString() + ": " + memberrole + ":" + account);
                                                }

                                                if (account != null && type != ClientType.None)
                                                {
                                                    account = account.ToLowerInvariant();
                                                    MSNLists msnlist = NSMessageHandler.ContactService.GetMSNList(memberrole);

                                                    if (bm.Deleted)
                                                    {
                                                        #region Members deleted in other clients.

                                                        if (type != ClientType.CircleMember)
                                                        {
                                                            if (HasMemberhip(updatedService.ServiceType, account, type, memberrole) &&
                                                                WebServiceDateTimeConverter.ConvertToDateTime(MembershipList[updatedService.ServiceType].Memberships[memberrole][Contact.MakeHash(account, type, WebServiceConstants.MessengerIndividualAddressBookId)].LastChanged)
                                                                < WebServiceDateTimeConverter.ConvertToDateTime(bm.LastChanged))
                                                            {
                                                                RemoveMemberhip(updatedService.ServiceType, account, type, memberrole);
                                                            }

                                                            if (NSMessageHandler.ContactList.HasContact(account, type))
                                                            {
                                                                Contact contact = NSMessageHandler.ContactList.GetContact(account, type);
                                                                contact.CID = cid;
                                                                if (contact.HasLists(msnlist))
                                                                {
                                                                    contact.RemoveFromList(msnlist);

                                                                    // Fire ReverseRemoved
                                                                    if (msnlist == MSNLists.ReverseList)
                                                                    {
                                                                        NSMessageHandler.ContactService.OnReverseRemoved(new ContactEventArgs(contact));
                                                                    }

                                                                    // Send a list remove event
                                                                    NSMessageHandler.ContactService.OnContactRemoved(new ListMutateEventArgs(contact, msnlist));
                                                                }
                                                            }
                                                        }

                                                        #endregion

                                                    }
                                                    else
                                                    {
                                                        #region Members added in other clients.

                                                        if (type != ClientType.CircleMember)
                                                        {
                                                            if (false == MembershipList[updatedService.ServiceType].Memberships.ContainsKey(memberrole) ||
                                                                /*new*/ false == MembershipList[updatedService.ServiceType].Memberships[memberrole].ContainsKey(Contact.MakeHash(account, type, WebServiceConstants.MessengerIndividualAddressBookId)) ||
                                                                /*probably membershipid=0*/ WebServiceDateTimeConverter.ConvertToDateTime(bm.LastChanged)
                                                                > WebServiceDateTimeConverter.ConvertToDateTime(MembershipList[updatedService.ServiceType].Memberships[memberrole][Contact.MakeHash(account, type, WebServiceConstants.MessengerIndividualAddressBookId)].LastChanged))
                                                            {
                                                                AddMemberhip(updatedService.ServiceType, account, type, memberrole, bm);
                                                            }

                                                            string displayname = bm.DisplayName == null ? account : bm.DisplayName;
                                                            Contact contact = NSMessageHandler.ContactList.GetContact(account, displayname, type);
                                                            contact.CID = cid;

                                                            if (!contact.HasLists(msnlist))
                                                            {
                                                                contact.AddToList(msnlist);

                                                                // Don't fire ReverseAdded(contact.Pending) here... It fires 2 times:
                                                                // The first is OnConnect after abSynchronized
                                                                // The second is here, not anymore here :)
                                                                // The correct place is in OnADLReceived.

                                                                // Send a list add event
                                                                NSMessageHandler.ContactService.OnContactAdded(new ListMutateEventArgs(contact, msnlist));
                                                            }
                                                        }

                                                        #endregion
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region ~Messenger
                                else
                                {
                                    foreach (Membership membership in serviceType.Memberships)
                                    {
                                        if (null != membership.Members)
                                        {
                                            string memberrole = membership.MemberRole;
                                            List<BaseMember> members = new List<BaseMember>(membership.Members);
                                            members.Sort(CompareBaseMembers);
                                            foreach (BaseMember bm in members)
                                            {
                                                string account = null;
                                                ClientType type = ClientType.None;

                                                switch (bm.Type)
                                                {
                                                    case MembershipType.Passport:
                                                        type = ClientType.PassportMember;
                                                        PassportMember pm = bm as PassportMember;
                                                        if (!pm.IsPassportNameHidden)
                                                        {
                                                            account = pm.PassportName;
                                                        }
                                                        break;

                                                    case MembershipType.Email:
                                                        type = ClientType.EmailMember;
                                                        account = ((EmailMember)bm).Email;
                                                        break;

                                                    case MembershipType.Phone:
                                                        type = ClientType.PhoneMember;
                                                        account = ((PhoneMember)bm).PhoneNumber;
                                                        break;

                                                    case MembershipType.Role:
                                                    case MembershipType.Service:
                                                    case MembershipType.Everyone:
                                                    case MembershipType.Partner:
                                                        account = bm.Type + "/" + bm.MembershipId;
                                                        break;

                                                    case MembershipType.Domain:
                                                        account = ((DomainMember)bm).DomainName;
                                                        break;

                                                    case MembershipType.Circle:
                                                        type = ClientType.CircleMember;
                                                        account = ((CircleMember)bm).CircleId;
                                                        Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, serviceType.Info.Handle.Type + " Membership " + bm.GetType().ToString() + ": " + memberrole + ":" + account);
                                                        break;
                                                }

                                                if (account != null)
                                                {
                                                    if (bm.Deleted)
                                                    {
                                                        RemoveMemberhip(updatedService.ServiceType, account, type, memberrole);
                                                    }
                                                    else
                                                    {
                                                        AddMemberhip(updatedService.ServiceType, account, type, memberrole, bm);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }

                            // Update service.LastChange
                            MembershipList[updatedService.ServiceType].Service = updatedService;
                        }
                    }
                }
            }

            return this;
        }

        private static int CompareBaseMembers(BaseMember x, BaseMember y)
        {
            return x.LastChanged.CompareTo(y.LastChanged);
        }


        #endregion

        #region Addressbook

        SerializableDictionary<string, ABFindContactsPagedResultTypeAB> abInfos = new SerializableDictionary<string, ABFindContactsPagedResultTypeAB>();
        SerializableDictionary<string, string> myproperties = new SerializableDictionary<string, string>(0);
        SerializableDictionary<Guid, GroupType> groups = new SerializableDictionary<Guid, GroupType>(0);
        SerializableDictionary<string, SerializableDictionary<Guid, ContactType>> abcontacts = new SerializableDictionary<string, SerializableDictionary<Guid, ContactType>>(0);

        SerializableDictionary<string, CircleInverseInfoType> circleResults = new SerializableDictionary<string, CircleInverseInfoType>(0);
        SerializableDictionary<long, string> wlConnections = new SerializableDictionary<long, string>(0);

        SerializableDictionary<string, ContactType> hiddenRepresentatives = new SerializableDictionary<string, ContactType>(0);
        SerializableDictionary<string, List<CircleMember>> circlesMembership = new SerializableDictionary<string, List<CircleMember>>(0);

        [NonSerialized]
        Dictionary<long, ContactType> contactTable = new Dictionary<long, ContactType>();

        [NonSerialized]
        Dictionary<string, long> wlInverseConnections = new Dictionary<string, long>();

        [NonSerialized]
        private CircleList pendingAcceptionCircleList;

        [NonSerialized]
        Dictionary<Guid, string> pendingCreateCircleList = new Dictionary<Guid, string>();

        /// <summary>
        /// Circles created by the library and waiting server's confirm.
        /// </summary>
        internal Dictionary<Guid, string> PendingCreateCircleList
        {
            get 
            { 
                return pendingCreateCircleList; 
            }
        }

        /// <summary>
        /// A collection of all circles which are pending acception.
        /// </summary>
        internal CircleList PendingAcceptionCircleList
        {
            get
            {
                if (pendingAcceptionCircleList == null && NSMessageHandler != null)
                    pendingAcceptionCircleList = new CircleList(NSMessageHandler);

                return pendingAcceptionCircleList;
            }
        }


        /// <summary>
        /// The relationship mapping from addressbook Ids to hidden represtative's CIDs.
        /// </summary>
        internal Dictionary<string, long> WLInverseConnections
        {
            get 
            { 
                return wlInverseConnections; 
            }
        }

        public SerializableDictionary<string, ContactType> HiddenRepresentatives
        {
            get { return hiddenRepresentatives; }
            set { hiddenRepresentatives = value; }
        }

        /// <summary>
        /// The relationship mapping from hidden represtative's CIDs to addressbook Ids.
        /// </summary>
        public SerializableDictionary<long, string> WLConnections
        {
            get 
            { 
                return wlConnections; 
            }

            set 
            { 
                wlConnections = value; 
            }
        }

        public SerializableDictionary<string, CircleInverseInfoType> CircleResults
        {
            get
            {
                return circleResults;
            }
            set
            {
                circleResults = value;
            }
        }

        [XmlElement("AddressBooksInfo")]
        public SerializableDictionary<string, ABFindContactsPagedResultTypeAB> AddressBooksInfo
        {
            get
            {
                return abInfos;
            }

            set
            {
                abInfos = value;
            }
        }

        /// <summary>
        /// Get the last changed date of a specific addressbook.
        /// </summary>
        /// <param name="abId">The Guid of AddreessBook.</param>
        /// <returns></returns>
        internal string GetAddressBookLastChange(Guid abId)
        {
            return GetAddressBookLastChange(abId.ToString("D"));

        }

        /// <summary>
        /// Get the last changed date of a specific addressbook.
        /// </summary>
        /// <param name="abId">The Guid of AddreessBook.</param>
        /// <returns></returns>
        internal string GetAddressBookLastChange(string abId)
        {
            string lowerId = abId.ToLowerInvariant();

            if (HasAddressBook(lowerId))
            {
                lock (AddressBooksInfo)
                    return AddressBooksInfo[lowerId].lastChange;
            }

            return WebServiceConstants.ZeroTime;
        }

        /// <summary>
        /// Set information for a specific addressbook.
        /// </summary>
        /// <param name="abId">AddressBook guid.</param>
        /// <param name="abHeader">The addressbook info.</param>
        internal void SetAddressBookInfo(Guid abId, ABFindContactsPagedResultTypeAB abHeader)
        {
            SetAddressBookInfo(abId.ToString("D"), abHeader);
        }

        /// <summary>
        /// Set information for a specific addressbook.
        /// </summary>
        /// <param name="abId">AddressBook guid.</param>
        /// <param name="abHeader">The addressbook info.</param>
        internal void SetAddressBookInfo(string abId, ABFindContactsPagedResultTypeAB abHeader)
        {
            string lowerId = abId.ToLowerInvariant();

            string compareTime = GetAddressBookLastChange(lowerId);

            try
            {
                
                DateTime oldTime = WebServiceDateTimeConverter.ConvertToDateTime(compareTime);
                DateTime newTime = WebServiceDateTimeConverter.ConvertToDateTime(abHeader.lastChange);
                if (oldTime >= newTime)
                {
                    Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "Update addressbook information skipped, abId: " +
                    abId + ", LastChange: " + abHeader.lastChange + ", compared with: " + compareTime);
                    return;  //Not necessary to update.
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(Settings.TraceSwitch.TraceError, "An error occured while setting AddressBook LastChange property, abId: " +
                    abId + ", LastChange: " + abHeader.lastChange + "\r\nError message: " + ex.Message);
                return;
            }

            SetAddressBookInfoToABInfoList(lowerId, abHeader);

            Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "Update addressbook information succeed, abId: " +
                    abId + ", LastChange: " + abHeader.lastChange + ", compared with: " + compareTime);
        }


        public SerializableDictionary<string, string> MyProperties
        {
            get
            {
                return myproperties;
            }
            set
            {
                myproperties = value;
            }
        }

        public SerializableDictionary<Guid, GroupType> Groups
        {
            get
            {
                return groups;
            }

            set
            {
                groups = value;
            }
        }

        /// <summary>
        /// The contact list for different address book pages.<br></br>
        /// The circle recreate procedure is based on this property.
        /// </summary>
        public SerializableDictionary<string, SerializableDictionary<Guid, ContactType>> AddressbookContacts
        {
            get
            {
                return abcontacts;
            }
            set
            {
                abcontacts = value;
            }
        }

        public void AddGroup(Dictionary<Guid, GroupType> range)
        {
            foreach (GroupType group in range.Values)
            {
                AddGroup(group);
            }
        }

        public void AddGroup(GroupType group)
        {
            Guid key = new Guid(group.groupId);
            if (groups.ContainsKey(key))
            {
                groups[key] = group;
            }
            else
            {
                groups.Add(key, group);
            }
        }

        public virtual void Add(string abId, Dictionary<Guid, ContactType> range)
        {
            string lowerId = abId.ToLowerInvariant();

            if (!abcontacts.ContainsKey(lowerId))
            {
                abcontacts.Add(lowerId, new SerializableDictionary<Guid, ContactType>(0));
            }

            foreach (Guid guid in range.Keys)
            {
                abcontacts[lowerId][guid] = range[guid];
            }
        }

        public XMLContactList Merge(ABFindContactsPagedResultType forwardList)
        {
            Initialize();

            #region AddressBook changed

            DateTime dt1 = WebServiceDateTimeConverter.ConvertToDateTime(GetAddressBookLastChange(forwardList.Ab.abId));
            DateTime dt2 = WebServiceDateTimeConverter.ConvertToDateTime(forwardList.Ab.lastChange);

            MergeIndividualAddressBook(forwardList);
            MergeGroupAddressBook(forwardList);
            #endregion

            //NO DynamicItems any more

            return this;
        }

        /// <summary>
        /// Update members for circles.
        /// </summary>
        /// <param name="forwardList"></param>
        internal void MergeGroupAddressBook(ABFindContactsPagedResultType forwardList)
        {
            #region Get Individual AddressBook Information (Circle information)

            if (forwardList.Ab != null && forwardList.Ab.abId != WebServiceConstants.MessengerIndividualAddressBookId && 
                forwardList.Ab.abInfo.AddressBookType == AddressBookType.Group &&
                WebServiceDateTimeConverter.ConvertToDateTime(GetAddressBookLastChange(forwardList.Ab.abId)) <
                WebServiceDateTimeConverter.ConvertToDateTime(forwardList.Ab.lastChange))
            {
                SetAddressBookInfo(forwardList.Ab.abId, forwardList.Ab);
                SaveAddressBookContactPage(forwardList.Ab.abId, forwardList.Contacts);

                //Create or update circle.
                Circle targetCircle = UpdateCircleFromAddressBook(forwardList.Ab.abId);

                if (targetCircle != null)
                {
                    //Update circle mebers.
                    UpdateCircleMembersFromAddressBookContactPage(targetCircle, Scenario.Initial);
                    switch (targetCircle.CircleRole)
                    {
                        case CirclePersonalMembershipRole.Admin:
                        case CirclePersonalMembershipRole.AssistantAdmin:
                        case CirclePersonalMembershipRole.Member:
                            AddCircleToCircleList(targetCircle);
                            break;

                        case CirclePersonalMembershipRole.StatePendingOutbound:
                            FireJoinCircleInvitationReceivedEvents(targetCircle);

                            break;
                    }

                    if (IsPendingCreateConfirmCircle(targetCircle.AddressBookId))
                    {
                        FireCreateCircleCompletedEvent(targetCircle); 
                    }

                    #region Print Info

                    Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "Getting non-default addressbook: \r\nId: " +
                                forwardList.Ab.abId + "\r\nName: " + forwardList.Ab.abInfo.name +
                                "\r\nType: " + forwardList.Ab.abInfo.AddressBookType + "\r\nMembers:");

                    string id = forwardList.Ab.abId + "@" + CircleString.DefaultHostDomain;
                    foreach (ContactType contact in forwardList.Contacts)
                    {
                        Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "PassportName: " + contact.contactInfo.passportName + ", DisplayName: " + contact.contactInfo.displayName + ", Type: " + contact.contactInfo.contactType);
                    }

                    Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "\r\n");

                    #endregion

                    SaveContactTable(forwardList.Contacts);

                    if (requestCircleCount > 0)
                    {
                        //Only the individual addressbook merge which contains new circles will cause this action.
                        requestCircleCount--;
                        if (requestCircleCount == 0)
                        {
                            Save();
                            NSMessageHandler.ContactService.SendInitialADL(Scenario.SendInitialCirclesADL);
                        }
                    }
                }
                else
                {
                    RemoveCircleInverseInfo(forwardList.Ab.abId);
                    RemoveAddressBookContatPage(forwardList.Ab.abId);
                    RemoveAddressBookInfo(forwardList.Ab.abId);

                    //Error? Save!
                    Save();

                    Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, 
                        "An error occured while merging the GroupAddressBook, addressbook info removed: " + 
                        forwardList.Ab.abId);
                }
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="circle"></param>
        /// <returns></returns>
        /// <remarks>This function must be called after the ContactTable and WLConnections are created.</remarks>
        private bool FireJoinCircleInvitationReceivedEvents(Circle circle)
        {
            CircleInviter invitor = GetCircleInviterFromNetworkInfo(SelecteContact(SelectWLConnection(circle.AddressBookId.ToString("D"))));
            if (invitor == null)
            {
                Trace.WriteLineIf(Settings.TraceSwitch.TraceError, "[MergeGroupAddressBook error] Cannot get the circle invitor, abId: " +
                    circle.AddressBookId.ToString("D") + ", an invitation may miss.");
                return false;
            }

            lock (PendingAcceptionCircleList)
            {
                PendingAcceptionCircleList.AddCircle(circle);
            }

            JoinCircleInvitationEventArgs joinArgs = new JoinCircleInvitationEventArgs(circle, invitor);
            NSMessageHandler.ContactService.OnJoinCircleInvitationReceived(joinArgs);
            return true;
        }

        private bool FireCreateCircleCompletedEvent(Circle newCircle)
        {
            Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "Newly created circle detected, create circle operation succeeded: " + newCircle);
            RemoveABIdFromPendingCreateCircleList(newCircle.AddressBookId);
            NSMessageHandler.ContactService.OnCreateCircleCompleted(new CircleEventArgs(newCircle));
            return true;
        }

        private ContactType SelectMeContactFromAddressBookContactPage(string abId)
        {
            if (!HasAddressBookContactPage(abId))
                return null;
            lock (AddressbookContacts)
                return SelectMeContactFromContactList((new List<ContactType>(AddressbookContacts[abId.ToLowerInvariant()].Values)).ToArray());
        }

        /// <summary>
        /// Get the addressbook's owner contact.
        /// </summary>
        /// <param name="contactList"></param>
        /// <returns></returns>
        private ContactType SelectMeContactFromContactList(ContactType[] contactList)
        {
            if (contactList == null)
                return null;

            foreach (ContactType contact in contactList)
            {
                if (contact.contactInfo != null)
                {
                    if (contact.contactInfo.contactType == MessengerContactType.Me)
                        return contact;
                }
            }

            return null;
        }

        internal Guid SelectSelfContactGuid(string abId)
        {
            ContactType self = SelectSelfContactFromAddressBookContactPage(abId);
            if (self == null)
                return Guid.Empty;

            return new Guid(self.contactId);
        }

        private ContactType SelectSelfContactFromAddressBookContactPage(string abId)
        {
            if (!HasAddressBookContactPage(abId))
                return null;

            if (NSMessageHandler.ContactList.Owner == null)
                return null;

            lock (AddressbookContacts)
                return SelectSelfContactFromContactList((new List<ContactType>(AddressbookContacts[abId.ToLowerInvariant()].Values)).ToArray(), NSMessageHandler.ContactList.Owner.Mail);
        }

        /// <summary>
        /// Get the owner of default addressbook in a certain addressbook page. This contact will used for exiting circle.
        /// </summary>
        /// <param name="contactList"></param>
        /// <param name="currentUserAccount"></param>
        /// <returns></returns>
        private ContactType SelectSelfContactFromContactList(ContactType[] contactList, string currentUserAccount)
        {
            if (contactList == null)
                return null;

            string lowerAccount = currentUserAccount.ToLowerInvariant();

            foreach (ContactType contact in contactList)
            {
                if (contact.contactInfo != null)
                {
                    if (contact.contactInfo.passportName.ToLowerInvariant() == lowerAccount)
                        return contact;
                }
            }

            return null;
        }

        /// <summary>
        /// Update the circle members and other information from a newly receive addressbook.
        /// This function can only be called after the contact page and WL connections were saved.
        /// </summary>
        /// <param name="abId"></param>
        /// <returns></returns>
        private Circle UpdateCircleFromAddressBook(string abId)
        {
            if (abId != WebServiceConstants.MessengerIndividualAddressBookId)
            {
                string lowerId = abId.ToLowerInvariant();
                

                ContactType meContact = SelectMeContactFromAddressBookContactPage(lowerId);
                long hiddenCID = SelectWLConnection(lowerId);
                ContactType hidden = SelecteContact(hiddenCID);
                CircleInverseInfoType inverseInfo = SelectCircleInverseInfo(lowerId);

                if (hidden == null)
                {
                    Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose,
                       "[UpdateCircleFromAddressBook] Cannot create circle since hidden representative not found in addressbook. ABId: "
                       + abId);
                    return null;
                }

                if (meContact == null)
                {
                    Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose,
                       "[UpdateCircleFromAddressBook] Cannot create circle since Me not found in addressbook. ABId: "
                       + abId);
                    return null;
                }

                if (inverseInfo == null)
                {
                    Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose,
                       "[UpdateCircleFromAddressBook] Cannot create circle since inverse info not found in circle result list. ABId: "
                       + abId);
                    return null;
                }

                Circle circle = NSMessageHandler.CircleList[new Guid(abId), inverseInfo.Content.Info.HostedDomain];

                if (circle == null)
                    circle = CreateCircle(meContact, hidden, inverseInfo);

                return circle;
            }

            return null;
        }

        private bool UpdateCircleMembersFromAddressBookContactPage(Circle circle, Scenario scene)
        {
            string lowerId = circle.AddressBookId.ToString("D").ToLowerInvariant();
            if (!HasAddressBookContactPage(lowerId))
                return false;

            Dictionary<long, ContactType> newContactList = null;
            Dictionary<long, Contact> oldContactInverseList = null;
            Contact[] oldContactList = null;

            bool isRestore = ((scene & Scenario.Restore) != Scenario.None);

            if (!isRestore)
            {
                newContactList = new Dictionary<long, ContactType>();
                oldContactInverseList = new Dictionary<long, Contact>();
                oldContactList = circle.ContactList.ToArray();
                foreach(Contact contact in oldContactList)
                {
                    oldContactInverseList[contact.CID] = contact;
                }
            }

            lock (AddressbookContacts)
            {
                SerializableDictionary<Guid, ContactType> page = AddressbookContacts[lowerId];

                foreach (ContactType contactType in page.Values)
                {
                    if (!isRestore)
                        newContactList[contactType.contactInfo.CID] = contactType;

                    if ((UpdateContact(contactType, lowerId, circle) & ReturnState.ProcessNextContact) == ReturnState.None)
                    {
                        Trace.WriteLineIf(Settings.TraceSwitch.TraceInfo, "[UpdateCircleMembersFromAddressBookContactPage] Create circle member failed: " +
                            contactType.contactInfo.passportName + ", UpdateContact returns false.");
                    }
                }
            }

            if (isRestore) return true;

            foreach (ContactType contactType in newContactList.Values)
            {
                if(contactType.contactInfo == null )continue;

                if (!oldContactInverseList.ContainsKey(contactType.contactInfo.CID) && 
                    circle.ContactList.HasContact(contactType.contactInfo.passportName, ClientType.PassportMember))
                {
                    circle.NSMessageHandler.ContactService.OnCircleMemberJoined(new CircleMemberEventArgs(circle, circle.ContactList[contactType.contactInfo.passportName, ClientType.PassportMember]));
                }
            }

            foreach (Contact contact in oldContactList)
            {
                if (!newContactList.ContainsKey(contact.CID))
                {
                    Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "Member " + contact.ToString() + " has left circle " + circle.ToString());
                    circle.ContactList.Remove(contact.Mail, contact.ClientType);
                    circle.NSMessageHandler.ContactService.OnCircleMemberLeft(new CircleMemberEventArgs(circle, contact));
                }
            }

            return true;
        }

        internal void MergeIndividualAddressBook(ABFindContactsPagedResultType forwardList)
        {
            #region Get Default AddressBook Information

            if (forwardList.Ab != null &&
                    WebServiceDateTimeConverter.ConvertToDateTime(GetAddressBookLastChange(forwardList.Ab.abId)) <
                    WebServiceDateTimeConverter.ConvertToDateTime(forwardList.Ab.lastChange)
                    && forwardList.Ab.abId == WebServiceConstants.MessengerIndividualAddressBookId)
            {
                Scenario scene = Scenario.None;

                if (IsContactTableEmpty())
                    scene = Scenario.Initial;
                else
                    scene = Scenario.DeltaRequest;

                #region Get groups

                if (null != forwardList.Groups)
                {
                    foreach (GroupType groupType in forwardList.Groups)
                    {
                        Guid key = new Guid(groupType.groupId);
                        if (groupType.fDeleted)
                        {
                            Groups.Remove(key);

                            ContactGroup contactGroup = NSMessageHandler.ContactGroups[groupType.groupId];
                            if (contactGroup != null)
                            {
                                NSMessageHandler.ContactGroups.RemoveGroup(contactGroup);
                                NSMessageHandler.ContactService.OnContactGroupRemoved(new ContactGroupEventArgs(contactGroup));
                            }
                        }
                        else
                        {
                            Groups[key] = groupType;

                            // Add a new group									
                            NSMessageHandler.ContactGroups.AddGroup(
                                new ContactGroup(System.Web.HttpUtility.UrlDecode(groupType.groupInfo.name), groupType.groupId, NSMessageHandler, groupType.groupInfo.IsFavorite));

                            // Fire the event
                            NSMessageHandler.ContactService.OnContactGroupAdded(
                                new ContactGroupEventArgs(NSMessageHandler.ContactGroups[groupType.groupId]));
                        }
                    }
                }

                #endregion

                #region Process Contacts

                SortedDictionary<long, long> newCIDList = new SortedDictionary<long, long>();
                Dictionary<string, CircleInverseInfoType> newInverseInfos = new Dictionary<string, CircleInverseInfoType>();

                SortedDictionary<long, CircleInverseInfoType> modifiedConnections = new SortedDictionary<long, CircleInverseInfoType>();

                if (forwardList.CircleResult != null && forwardList.CircleResult.Circles != null)
                {
                    foreach (CircleInverseInfoType info in forwardList.CircleResult.Circles)
                    {
                        string abId = info.Content.Handle.Id.ToLowerInvariant();
                        long CID = SelectWLConnection(abId);
                        if (HasWLConnection(abId))
                        {
                            if (!modifiedConnections.ContainsKey(CID))
                            {
                                modifiedConnections[CID] = info;
                            }
                        }
                        else
                        {
                            newInverseInfos[info.Content.Handle.Id.ToLowerInvariant()] = info;
                        }
                    }
                }

                if (null != forwardList.Contacts)
                {
                    foreach (ContactType contactType in forwardList.Contacts)
                    {
                        if (null != contactType.contactInfo)
                        {
                            SetContactToAddressBookContactPage(forwardList.Ab.abId, contactType);

                            /*
                             * Circle update rules:
                             * 1. If your own circle has any update (i.e. adding or deleting members), no hidden representative will be changed, only circle inverse info will be provided.
                             * 2. If a remote owner removes you from his circle, the hidden representative of that circle will change its RelationshipState to 2, circle inverse info will not provided.
                             * 3. If a remote owner re-adds you into a circle which you've left before, the hidden representative will be created, its relationshipState is 3, and the circle inverse info will be provided.
                             * 4. If you are already in a circle, the circle's owner removed you, then add you back, the hidden representative's RelationshipState property in NetworkInfo will change from 2 to 3.
                             * 5. If a remote contact has left your own circle, hidden representative will not change but circle inverse info will be provided.
                             * 6. If you delete your own circle, the hidden representative's contactType will change, and circle reverse info will be provided.
                             * 7. If you create a circle, the hidden representative will also create and circle inverse info will be provided.
                             * 8. If a remote owner invites you to join a circle, the hidden representative will be created, its relationshipState is 1 and circle inverse info will be provided, Role = StatePendingOutbound.
                             */
                            long CID = contactType.contactInfo.CID;

                            if (HasWLConnection(CID))
                            {
                                modifiedConnections[CID] = SelectCircleInverseInfo(SelectWLConnection(CID));

                                ContactType savedContact = SelecteContact(contactType.contactInfo.CID);
                                //A deleted or modified circle; We are NOT in initial scene.

                                if (savedContact.contactInfo.contactType == MessengerContactType.Circle)
                                {
                                    if (savedContact.contactInfo.contactType != contactType.contactInfo.contactType)
                                    {
                                        //Owner deleted circles found.
                                        //The members in the circle which this contact represents are all livepending contacts.
                                        //Or, the circle this contact represents has no member.
                                        //ModifyCircles(contactType, forwardList.CircleResult);

                                        Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "A deleted circle found: contactType: " + contactType.contactInfo.contactType + "\r\n " +
                                            "CID: " + contactType.contactInfo.CID.ToString() + "\r\n " +
                                            "PassportName: " + contactType.contactInfo.passportName + "\r\n " +
                                            "DomainTag: " + GetHiddenRepresentativeDomainTag(contactType) + "\r\n " +
                                            "RelationshipState: " + GetCircleMemberRelationshipStateFromNetworkInfo(contactType.contactInfo.NetworkInfoList).ToString()
                                            + "\r\n");
                                    }
                                    else
                                    {
                                        //We may remove by the circle owner, so a circle is deleted.
                                        //ModifyCircles(contactType, forwardList.CircleResult);

                                        Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "A modified circle found: contactType: " + contactType.contactInfo.contactType + "\r\n " +
                                            "CID: " + contactType.contactInfo.CID.ToString() + "\r\n " +
                                            "PassportName: " + contactType.contactInfo.passportName + "\r\n " +
                                            "DomainTag: " + GetHiddenRepresentativeDomainTag(contactType) + "\r\n " +
                                            "RelationshipState: " + GetCircleMemberRelationshipStateFromNetworkInfo(contactType.contactInfo.NetworkInfoList).ToString()
                                            + "\r\n");
                                    }

                                    continue;
                                }
                            }
                            else
                            {

                                if (contactType.contactInfo.contactType == MessengerContactType.Circle)
                                {
                                    RelationshipState state = GetCircleMemberRelationshipStateFromNetworkInfo(contactType.contactInfo.NetworkInfoList);

                                    switch (state)
                                    {
                                        case RelationshipState.Accepted:
                                        case RelationshipState.WaitingResponse:
                                            newCIDList[CID] = CID;
                                            break;
                                    }

                                    //We get the hidden representative of a new circle.
                                    Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "A circle contact found: contactType: " + contactType.contactInfo.contactType + "\r\n " +
                                        "CID: " + contactType.contactInfo.CID.ToString() + "\r\n " +
                                        "PassportName: " + contactType.contactInfo.passportName + "\r\n " +
                                        "DomainTag: " + GetHiddenRepresentativeDomainTag(contactType) + "\r\n " +
                                        "RelationshipState: " + GetCircleMemberRelationshipStateFromNetworkInfo(contactType.contactInfo.NetworkInfoList).ToString()
                                        + "\r\n");
                                    continue;
                                }
                            }

                            Contact contact = NSMessageHandler.ContactList.GetContactByGuid(new Guid(contactType.contactId));

                            if (contactType.fDeleted)
                            {
                                //The contact was deleted.

                                RemoveContactFromAddressBook(forwardList.Ab.abId, new Guid(contactType.contactId));

                                if (contact != null)
                                {
                                    contact.RemoveFromList(MSNLists.ForwardList);
                                    NSMessageHandler.ContactService.OnContactRemoved(new ListMutateEventArgs(contact, MSNLists.ForwardList));

                                    contact.Guid = Guid.Empty;
                                    contact.SetIsMessengerUser(false);

                                    PresenceStatus oldStatus = contact.Status;
                                    contact.SetStatus(PresenceStatus.Offline);  //Force the contact offline.
                                    NSMessageHandler.OnContactStatusChanged(new ContactStatusChangedEventArgs(contact, oldStatus));
                                    NSMessageHandler.OnContactOffline(new ContactEventArgs(contact));


                                    if (MSNLists.None == contact.Lists)
                                    {
                                        NSMessageHandler.ContactList.Remove(contact.Mail, contact.ClientType);
                                        contact.NSMessageHandler = null;
                                    }
                                }
                            }
                            else
                            {
                                UpdateContact(contactType);
                                NSMessageHandler.ContactService.OnContactAdded(new ListMutateEventArgs(contact, MSNLists.ForwardList));
                            }
                        }
                    }
                }

                if (forwardList.Ab != null)
                {
                    // Update lastchange
                    SetAddressBookInfo(forwardList.Ab.abId, forwardList.Ab);
                }

                SaveContactTable(forwardList.Contacts);
                if (forwardList.CircleResult != null)
                    SaveCircleInverseInfo(forwardList.CircleResult.Circles);


                ProcessCircles(modifiedConnections, newCIDList, newInverseInfos, scene);
            }

            #endregion

            #endregion
        }

        private void ProcessCircles(SortedDictionary<long, CircleInverseInfoType> modifiedConnections, SortedDictionary<long, long> newCIDList, Dictionary<string, CircleInverseInfoType> newInverseInfos, Scenario scene)
        {
            int[] result = new int[] { 0, 0 };
            //We must process modified circles first.
            result = ProcessModifiedCircles(modifiedConnections, scene | Scenario.ModifiedCircles);
            result = ProcessNewConnections(new List<long>(newCIDList.Keys), new List<CircleInverseInfoType>(newInverseInfos.Values), scene | Scenario.NewCircles);
        }

        private int[] ProcessNewConnections(List<long> sortedCIDs, List<CircleInverseInfoType> sortedInfos, Scenario scene)
        {
            int added = 0;
            int pending = 0;

            SaveWLConnection(sortedCIDs, sortedInfos.ToArray());

            string[] abIds = SelectWLConnection(sortedCIDs, RelationshipState.Accepted);
            RequestCircles(abIds, RelationshipState.Accepted, scene);
            added = abIds.Length;

            abIds = SelectWLConnection(sortedCIDs, RelationshipState.WaitingResponse);
            RequestCircles(abIds, RelationshipState.WaitingResponse, scene);
            pending = abIds.Length;

            return new int[] { added, pending };
        }

        private int[] ProcessModifiedCircles(SortedDictionary<long, CircleInverseInfoType> modifiedConnections, Scenario scene)
        {
            int deleted = 0;
            int reAdded = 0;

            SortedDictionary<long, CircleInverseInfoType> connectionClone = new SortedDictionary<long, CircleInverseInfoType>(modifiedConnections);
            foreach (long CID in modifiedConnections.Keys)
            {
                ContactType hidden = SelecteContact(CID);
                if (
                    modifiedConnections[CID].Deleted ||   //Local owner delete circle.
                    hidden.contactInfo.contactType != MessengerContactType.Circle ||  //Remote owner delete circle
                    GetCircleMemberRelationshipStateFromNetworkInfo(hidden.contactInfo.NetworkInfoList) == RelationshipState.Left //Remote owner delete local user from circle.
                    )
                {
                    RemoveCircle(CID, modifiedConnections[CID].Content.Handle.Id);
                    connectionClone.Remove(CID);
                    deleted++;
                }
            }

            List<long> sortedCIDs = new List<long>(connectionClone.Keys);
            List<CircleInverseInfoType> sortedInfos = new List<CircleInverseInfoType>(connectionClone.Values);
            SaveWLConnection(sortedCIDs, sortedInfos.ToArray());

            string[] abIds = SelectWLConnection(sortedCIDs, RelationshipState.Accepted);  //Select the re-added circles.
            RequestCircles(abIds, RelationshipState.Accepted, scene);
            reAdded = abIds.Length;

            return new int[] { deleted, reAdded };
        }

        private bool SaveWLConnection(List<long> sortedCIDList, CircleInverseInfoType[] inverseList)
        {
            if (inverseList == null)
                return false;

            if (sortedCIDList.Count != inverseList.Length)
                return false;

            lock (WLConnections)
            {
                lock (WLInverseConnections)
                {
                    for (int cnt = 0; cnt < sortedCIDList.Count; cnt++)
                    {
                        CircleInverseInfoType inverseInfo = inverseList[cnt];
                        long CID = sortedCIDList[cnt];
                        WLConnections[CID] = inverseInfo.Content.Handle.Id.ToLowerInvariant();
                        WLInverseConnections[inverseInfo.Content.Handle.Id.ToLowerInvariant()] = CID;
                    }
                }
            }

            return true;

        }

        private bool SaveAddressBookContactPage(string abId, ContactType[] contacts)
        {
            if (contacts == null)
                return false;

            lock (AddressbookContacts)
            {
                SerializableDictionary<Guid, ContactType> page = new SerializableDictionary<Guid, ContactType>(0);
                AddressbookContacts[abId.ToLowerInvariant()] = page;
                foreach (ContactType contact in contacts)
                {
                    page[new Guid(contact.contactId)] = contact;
                }
            }

            return true;
        }

        private void SaveCircleInverseInfo(CircleInverseInfoType[] inverseInfoList)
        {
            List<string> modifiedCircles = new List<string>(0);
            if (inverseInfoList != null)
            {
                foreach (CircleInverseInfoType circle in inverseInfoList)
                {
                    string lowerId = circle.Content.Handle.Id.ToLowerInvariant();

                    lock (CircleResults)
                    {

                        CircleResults[lowerId] = circle;
                    }
                }
            }

        }

        private void SaveContactTable(ContactType[] contacts)
        {
            if (contacts == null)
                return;

            lock (contactTable)
            {
                foreach (ContactType contact in contacts)
                {
                    if (contact.contactInfo != null)
                    {
                        contactTable[contact.contactInfo.CID] = contact;
                    }
                }
            }
        }

        /// <summary>
        /// Clean up the saved circle addressbook information.
        /// </summary>
        internal void ClearCircleInfos()
        {
            lock (CircleResults)
                CircleResults.Clear();

            lock (AddressBooksInfo)
                AddressBooksInfo.Clear();

            lock (AddressbookContacts)
                AddressbookContacts.Clear();

            lock (contactTable)
                contactTable.Clear();

            lock (WLConnections)
                WLConnections.Clear();

            lock (WLInverseConnections)
                WLInverseConnections.Clear();

        }

        /// <summary>
        /// 1. RemoveAddressBookContatPage
        /// 2. RemoveAddressBookInfo
        /// 3. RemoveCircleInverseInfo
        /// 4. BreakWLConnection
        /// 5. RemoveCircle
        /// </summary>
        /// <param name="initiatorCID"></param>
        /// <param name="abId"></param>
        /// <returns></returns>
        internal bool RemoveCircle(long initiatorCID, string abId)
        {
            if (HasWLConnection(initiatorCID) && !string.IsNullOrEmpty(abId))
            {
                CircleInverseInfoType inversoeInfo = SelectCircleInverseInfo(abId);
                Circle tempCircle = null;
                if (inversoeInfo != null)
                {
                    tempCircle = NSMessageHandler.CircleList[new Guid(abId), inversoeInfo.Content.Info.HostedDomain];
                }

                //1. Remove corresponding addressbook page.
                RemoveAddressBookContatPage(abId);

                //2. Remove addressbook info.
                RemoveAddressBookInfo(abId);

                //3. Remove circle inverse info.
                RemoveCircleInverseInfo(abId);

                //4. Break the connection between hidden representative and addressbook.
                BreakWLConnection(initiatorCID);

                //5. Remove the presentation data structure for a circle.
                NSMessageHandler.CircleList.RemoveCircle(new Guid(abId), CircleString.DefaultHostDomain);

                if (tempCircle != null)
                {
                    NSMessageHandler.ContactService.OnExitCircleCompleted(new CircleEventArgs(tempCircle));
                }

                return true;
            }

            return false;
        }

        internal bool RemoveCircleInverseInfo(string abId)
        {
            lock (CircleResults)
                return CircleResults.Remove(abId.ToLowerInvariant());
        }

        /// <summary>
        /// Break the CID-AbID relationship of hidden representative to addressbook.
        /// </summary>
        /// <param name="CID"></param>
        /// <returns></returns>
        private bool BreakWLConnection(long CID)
        {
            if (!HasWLConnection(CID))
                return false;

            string abId = SelectWLConnection(CID);

            lock (WLInverseConnections)
            {
                lock (WLConnections)
                {
                    return (WLConnections.Remove(CID) && WLInverseConnections.Remove(abId));
                }
            }
        }

        private bool RemoveABIdFromPendingCreateCircleList(Guid abId)
        {
            lock (PendingCreateCircleList)
                return PendingCreateCircleList.Remove(abId);
        }


        /// <summary>
        /// Get a circle addressbook by addressbook identifier.
        /// </summary>
        /// <param name="abId"></param>
        /// <param name="state"></param>
        /// <param name="scene"></param>
        /// <returns></returns>
        private bool RequestAddressBookByABId(string abId, RelationshipState state, Scenario scene)
        {
            Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "Requesting AddressBook by ABId: " + abId + ", Scenario: " + scene.ToString());

            abId = abId.ToLowerInvariant();

            abHandleType individualAB = new abHandleType();
            individualAB.ABId = abId;
            individualAB.Cid = 0;
            individualAB.Puid = 0;

            switch (state)
            {
                case RelationshipState.Accepted:
                    requestCircleCount++;
                    NSMessageHandler.ContactService.abRequest(PartnerScenario.Initial, individualAB, null);
                    break;
                case RelationshipState.WaitingResponse:
                    NSMessageHandler.ContactService.abRequest(PartnerScenario.Initial, individualAB, null);
                    break;
            }

            return true;
        }


        /// <summary>
        /// Create a circle.
        /// </summary>
        /// <param name="hiddenRepresentative"></param>
        /// <param name="me"></param>
        /// <param name="inverseInfo"></param>
        /// <returns></returns>
        private Circle CreateCircle(ContactType me, ContactType hiddenRepresentative, CircleInverseInfoType inverseInfo)
        {
            if (hiddenRepresentative.contactInfo == null)
            {
                return null;
            }

            if (hiddenRepresentative.contactInfo.NetworkInfoList.Length == 0)
            {
                return null;
            }

            if (hiddenRepresentative.contactInfo.contactType != MessengerContactType.Circle)
            {
                return null;
            }
            

            return new Circle(me,hiddenRepresentative, inverseInfo, NSMessageHandler);
        }

        private bool AddCircleToCircleList(Circle circle)
        {
            bool result = NSMessageHandler.CircleList.AddCircle(circle);

            lock (PendingAcceptionCircleList)
            {
                if (PendingAcceptionCircleList[circle.AddressBookId, circle.HostDomain] != null)
                {
                    NSMessageHandler.ContactService.OnJoinedCircleCompleted(new CircleEventArgs(NSMessageHandler.CircleList[circle.AddressBookId, circle.HostDomain]));
                }

                PendingAcceptionCircleList.RemoveCircle(circle.AddressBookId, circle.HostDomain);
            }

            return result;
        }

        /// <summary>
        /// Restore the inverse list of WLConnections.
        /// </summary>
        private void RestoreWLConnections()
        {
            if (WLInverseConnections == null)
                wlInverseConnections = new Dictionary<string, long>();

            //Restore, lock is not needed.
            foreach (long CID in WLConnections.Keys)
            {
                WLInverseConnections[WLConnections[CID]] = CID;
            }
        }

        private bool RestoreCircles(long[] CIDs, RelationshipState state)
        {
            if ( CIDs == null)
                return false;

            foreach (long CID in CIDs)
            {
                RestoreCircleFromAddressBook(SelectWLConnection(CID), SelecteContact(CID), state);
            }

            return true;
        }

        private bool RestoreCircleFromAddressBook(string abId, ContactType hidden, RelationshipState state)
        {
            string lowerId = abId.ToLowerInvariant();

            if (lowerId == WebServiceConstants.MessengerIndividualAddressBookId)
                return true;

            if (!HasAddressBook(lowerId))
            {
                Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "[RestoreCircleFromAddressBook] failed, cannot find specific addressbook :" + lowerId);
                return false;
            }

            if (!AddressbookContacts.ContainsKey(lowerId))
            {
                Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "[RestoreCircleFromAddressBook] failed, cannot find specific addressbook contact group:" + lowerId);
                return false;
            }

            //We use addressbook list to boot the restore procedure.
            ContactType me = SelectMeContactFromContactList(new List<ContactType>(AddressbookContacts[lowerId].Values).ToArray());
            CircleInverseInfoType inverseInfo = SelectCircleInverseInfo(lowerId);

            if (me == null)
            {
                Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "[RestoreCircleFromAddressBook] Me Contact not found, restore circle failed:" + lowerId);
                return false;
            }

            if (hidden == null)
            {
                Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "[RestoreCircleFromAddressBook] Hidden Representative not found, restore circle failed:" + lowerId);
                return false;
            }

            if (inverseInfo == null)
            {
                Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "[RestoreCircleFromAddressBook] Circle inverse info not found, restore circle failed:" + lowerId);
                return false;
            }

            if (NSMessageHandler.CircleList[new Guid(lowerId), CircleString.DefaultHostDomain] != null)
            {
                Trace.WriteLineIf(Settings.TraceSwitch.TraceVerbose, "[RestoreCircleFromAddressBook] circle already exists, restore skipped:" + lowerId);
                return false;
            }

            Circle circle = CreateCircle(me, hidden, inverseInfo);
            UpdateCircleMembersFromAddressBookContactPage(circle, Scenario.Restore);

            switch (circle.CircleRole)
            {
                case CirclePersonalMembershipRole.Admin:
                case CirclePersonalMembershipRole.AssistantAdmin:
                case CirclePersonalMembershipRole.Member:
                    AddCircleToCircleList(circle);
                    break;
                case CirclePersonalMembershipRole.StatePendingOutbound:
                    FireJoinCircleInvitationReceivedEvents(circle);
                    break;
            }

            return true;

        }


        private CircleInviter GetCircleInviterFromNetworkInfo(ContactType contact)
        {
            CircleInviter initator = null;

            if (contact.contactInfo.NetworkInfoList.Length > 0)
            {
                foreach (NetworkInfoType networkInfo in contact.contactInfo.NetworkInfoList)
                {
                    if (networkInfo.DomainId == 1)
                    {
                        if (networkInfo.InviterCIDSpecified)
                        {
                            ContactType inviter = SelecteContact(networkInfo.InviterCID);
                            if (inviter == null)
                            {
                                Trace.WriteLineIf(Settings.TraceSwitch.TraceError, "[GetCircleInviterFromNetworkInfo] Cannot create circle inviter from CID: " + networkInfo.InviterCID + ", contact not found.");
                                return null;
                            }

                            initator = new CircleInviter(inviter, networkInfo.InviterMessage);
                        }
                        else
                        {
                            initator = new CircleInviter(networkInfo.InviterEmail, networkInfo.InviterName, networkInfo.InviterMessage);
                        }
                    }
                }
            }

            return initator;
        }

        /// <summary>
        /// Use msn webservices to get addressbooks.
        /// </summary>
        /// <param name="abIds"></param>
        /// <param name="state"></param>
        /// <param name="scene"></param>
        private void RequestCircles(string[] abIds, RelationshipState state, Scenario scene)
        {
            if (abIds == null)
                return;

            foreach (string abId in abIds)
            {
                RequestAddressBookByABId(abId, state, scene);
            }

        }

        private ReturnState UpdateContact(ContactType contactType)
        {
            return UpdateContact(contactType, WebServiceConstants.MessengerIndividualAddressBookId, null);
        }

        private ReturnState UpdateContact(ContactType contactType, string abId, Circle circle)
        {
            if (contactType.contactInfo == null)
            {
                Trace.WriteLineIf(Settings.TraceSwitch.TraceError, "Cannot update contact, contact info is null.");
                return ReturnState.UpdateError;
            }

            contactInfoType cinfo = contactType.contactInfo;
            ClientType type = ClientType.PassportMember;
            string account = cinfo.passportName;
            string displayName = cinfo.displayName;
            bool isMessengeruser = cinfo.isMessengerUser;
            string lowerId = abId.ToLowerInvariant();
            ReturnState returnValue = ReturnState.ProcessNextContact;
            ContactList contactList = null;
            bool isDefaultAddressBook = (lowerId == WebServiceConstants.MessengerIndividualAddressBookId);

            if (cinfo.emails != null && account == null && cinfo != null)
            {
                foreach (contactEmailType cet in cinfo.emails)
                {
                    if (cet.isMessengerEnabled)
                    {
                        type = (ClientType)Enum.Parse(typeof(ClientType), cet.Capability);
                        account = cet.email;
                        isMessengeruser |= cet.isMessengerEnabled;
                        displayName = account;
                        break;
                    }
                }
            }

            if (cinfo.phones != null && account == null)
            {
                foreach (contactPhoneType cpt in cinfo.phones)
                {
                    if (cpt.isMessengerEnabled)
                    {
                        type = ClientType.PhoneMember;
                        account = cpt.number;
                        isMessengeruser |= cpt.isMessengerEnabled;
                        displayName = account;
                        break;
                    }
                }
            }

            if (account != null)
            {
                account = account.ToLowerInvariant();
                if (cinfo.contactType != MessengerContactType.Me)
                {
                    #region Contacts other than owner

                    Contact contact = null;

                    if (isDefaultAddressBook)
                    {
                        contact = NSMessageHandler.ContactList.GetContact(account, type);
                        contactList = NSMessageHandler.ContactList;
                    }
                    else
                    {
                        if ( circle == null)
                        {
                            Trace.WriteLineIf(Settings.TraceSwitch.TraceError, "Cannot update contact: " + account + " in addressbook: " + abId);

                            //This means we are restoring contacts from mcl file.
                            //We need to retore the circle first, then initialize this contact again.
                            return ReturnState.UpdateError;
                        }

                        CirclePersonalMembershipRole membershipRole = GetCircleMemberRoleFromNetworkInfo(cinfo.NetworkInfoList);
                        contact = circle.ContactList.GetContact(account, type);
                        contactList = circle.ContactList;
                        contact.CircleRole = membershipRole;
                        string tempName = GetCircleMemberDisplayNameFromNetworkInfo(cinfo.NetworkInfoList);
                        if (!string.IsNullOrEmpty(tempName))
                            displayName = tempName;
                    }

                    contact.Guid = new Guid(contactType.contactId);
                    contact.CID = Convert.ToInt64(cinfo.CID);
                    contact.ContactType = cinfo.contactType;
                    contact.SetHasSpace(cinfo.hasSpace);   //DONOT trust this
                    contact.SetComment(cinfo.comment);
                    contact.SetIsMessengerUser(isMessengeruser);
                    contact.SetMobileAccess(cinfo.isMobileIMEnabled);
                    contact.UserTile = GetUserTitleURLFromWindowsLiveNetworkInfo(contactType);
                    SetContactPhones(contact, cinfo);
                    contact.SetNickName(GetContactNickName(contactType));


                    if (contact.IsMessengerUser)
                    {
                        contact.AddToList(MSNLists.ForwardList); //IsMessengerUser is only valid in AddressBook member
                    }

                    if (!String.IsNullOrEmpty(displayName))
                    {
                        if (contact.Name == contact.Mail && displayName != contact.Mail)
                            contact.SetName(displayName);
                    }


                    if (cinfo.groupIds != null)
                    {
                        foreach (string groupId in cinfo.groupIds)
                        {
                            contact.ContactGroups.Add(NSMessageHandler.ContactGroups[groupId]);
                        }
                    }

                    if (cinfo.groupIdsDeleted != null)
                    {
                        foreach (string groupId in cinfo.groupIdsDeleted)
                        {
                            contact.ContactGroups.Remove(NSMessageHandler.ContactGroups[groupId]);
                        }
                    } 

                    #endregion

                    #region Filter yourself and members who alrealy left this circle.
                    bool needsDelete = false;

                    RelationshipState relationshipState = GetCircleMemberRelationshipStateFromNetworkInfo(cinfo.NetworkInfoList);
                    if (((relationshipState & RelationshipState.Rejected) != RelationshipState.None|| 
                        relationshipState == RelationshipState.None) &&
                        isDefaultAddressBook == false)
                    {
                        //Members who already left.
                        needsDelete |= true;
                    }

                    if (cinfo.IsHiddenSpecified && cinfo.IsHidden)
                    {
                        needsDelete |= true;
                    }

                    if (account == NSMessageHandler.ContactList.Owner.Mail.ToLowerInvariant() && 
                        cinfo.NetworkInfoList != null && 
                        type == NSMessageHandler.ContactList.Owner.ClientType &&
                        isDefaultAddressBook == false)
                    {
                        //This is a self contact. If we need to left a circle, we need its contactId.
                        //The exit circle operation just delete this contact from addressbook.
                        needsDelete |= true;
                    }

                    if (contactType.fDeleted)
                    {
                        needsDelete |= true;
                    }

                    if (needsDelete)
                    {
                        contactList.Remove(account, type);
                    }

                    #endregion
                }
                else
                {
                    #region Update owner and Me contact
		
                    Owner owner = null;

                    if (lowerId == WebServiceConstants.MessengerIndividualAddressBookId)
                    {
                        owner = NSMessageHandler.ContactList.Owner;
                        if (owner == null)
                        {
                            owner = new Owner(abId, cinfo.passportName, NSMessageHandler);
                            NSMessageHandler.ContactList.SetOwner(owner);
                        }
                    }
                    else
                    {
                        if (circle == null)
                        {
                            Trace.WriteLineIf(Settings.TraceSwitch.TraceError, "Cannot update owner: " + account + " in addressbook: " + abId);

                            return ReturnState.UpdateError;
                        }

                        owner = circle.ContactList.Owner;
                    }

                    if (displayName == owner.Mail && !String.IsNullOrEmpty(owner.Name))
                    {
                        displayName = owner.Name;
                    }

                    owner.Guid = new Guid(contactType.contactId);
                    owner.CID = Convert.ToInt64(cinfo.CID);
                    owner.ContactType = cinfo.contactType;
                    owner.SetName(displayName);
                    owner.SetNickName(GetContactNickName(contactType));
                    owner.UserTile = GetUserTitleURLFromWindowsLiveNetworkInfo(contactType);
                    SetContactPhones(owner, cinfo);
	#endregion

                    if (null != cinfo.annotations && lowerId == WebServiceConstants.MessengerIndividualAddressBookId)
                    {
                        foreach (Annotation anno in cinfo.annotations)
                        {
                            MyProperties[anno.Name] = anno.Value;
                        }
                    }

                    InitializeMyProperties();
                }
            }

            return returnValue;
        }

        private bool SetContactPhones(Contact contact, contactInfoType cinfo)
        {
            if (cinfo == null)
                return false;

            if (cinfo.phones == null)
                return false;


            foreach (contactPhoneType cp in cinfo.phones)
            {
                switch (cp.contactPhoneType1)
                {
                    case ContactPhoneTypeType.ContactPhoneMobile:
                        contact.SetMobilePhone(cp.number);
                        break;

                    case ContactPhoneTypeType.ContactPhonePersonal:
                        contact.SetHomePhone(cp.number);
                        break;

                    case ContactPhoneTypeType.ContactPhoneBusiness:
                        contact.SetWorkPhone(cp.number);
                        break;
                }
            }

            return true;

        }

        /// <summary>
        /// Get a contact's nick name from it's Annotations.
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        private string GetContactNickName(ContactType contact)
        {
            if (contact.contactInfo == null)
                return string.Empty;

            if (contact.contactInfo.annotations == null)
                return string.Empty;

            foreach(Annotation anno in contact.contactInfo.annotations)
            {
                if(anno.Name == AnnotationNames.AB_NickName)
                {
                    return anno.Value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Get windows live user title url.
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        private Uri GetUserTitleURLFromWindowsLiveNetworkInfo(ContactType contact)
        {
            string returnURL = GetUserTitleURLByDomainIdFromNetworkInfo(contact, DomainIds.WLDomain);
            try
            {
                Uri urlResult = null;
                if (Uri.TryCreate(returnURL, UriKind.Absolute, out urlResult))
                    return urlResult;
            }
            catch (Exception)
            {
                
            }

            return null;
        }

        /// <summary>
        /// Get user title url.
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        private string GetUserTitleURLByDomainIdFromNetworkInfo(ContactType contact, int domainId)
        {
            if (contact.contactInfo == null)
                return string.Empty;
            if (contact.contactInfo.NetworkInfoList == null)
                return string.Empty;

            foreach (NetworkInfoType info in contact.contactInfo.NetworkInfoList)
            {
                if (info.DomainIdSpecified && info.DomainId == domainId)
                {
                    if (!string.IsNullOrEmpty(info.UserTileURL))
                    {
                        return info.UserTileURL;
                    }
                }
            }

            return string.Empty;

        }

        /// <summary>
        /// Get a contact's RelationshipState property by providing DomainId = 1 and RelationshipType = 5.
        /// </summary>
        /// <param name="infoList"></param>
        /// <returns></returns>
        private RelationshipState GetCircleMemberRelationshipStateFromNetworkInfo(NetworkInfoType[] infoList)
        {
            return (RelationshipState)GetContactRelationshipStateFromNetworkInfo(infoList, DomainIds.WLDomain, RelationshipTypes.CircleGroup);
        }

        /// <summary>
        /// Get a contact's RelationshipState property by providing DomainId and RelationshipType
        /// </summary>
        /// <param name="infoList"></param>
        /// <param name="domainId"></param>
        /// <param name="relationshipType"></param>
        /// <returns></returns>
        private int GetContactRelationshipStateFromNetworkInfo(NetworkInfoType[] infoList, int domainId, int relationshipType)
        {
            if (infoList == null)
                return 0;

            foreach (NetworkInfoType info in infoList)
            {
                if (info.RelationshipTypeSpecified && info.DomainIdSpecified && info.RelationshipStateSpecified)
                {
                    if (info.DomainId == domainId && info.RelationshipType == relationshipType)
                    {
                        return info.RelationshipState;
                    }
                }
            }

            return 0;
        }

        private string GetCircleMemberDisplayNameFromNetworkInfo(NetworkInfoType[] infoList)
        {
            return GetContactDisplayNameFromNetworkInfo(infoList, DomainIds.WLDomain, RelationshipTypes.CircleGroup);
        }

        private string GetContactDisplayNameFromNetworkInfo(NetworkInfoType[] infoList, int domainId, int relationshipType)
        {
            if (infoList == null)
                return string.Empty;

            foreach (NetworkInfoType info in infoList)
            {
                if (info.RelationshipTypeSpecified && info.DomainIdSpecified && !string.IsNullOrEmpty(info.DisplayName))
                {
                    if (info.DomainId == domainId && info.RelationshipType == relationshipType)
                    {
                        return info.DisplayName;
                    }
                }
            }

            return string.Empty;
        }


        private CirclePersonalMembershipRole GetCircleMemberRoleFromNetworkInfo(NetworkInfoType[] infoList)
        {
            return (CirclePersonalMembershipRole)GetContactRelationshipRoleFromNetworkInfo(infoList, DomainIds.WLDomain, RelationshipTypes.CircleGroup);
        }

        private int GetContactRelationshipRoleFromNetworkInfo(NetworkInfoType[] infoList, int domainId, int relationshipType)
        {
            if (infoList == null)
                return 0;

            foreach (NetworkInfoType info in infoList)
            {
                if (info.RelationshipTypeSpecified && info.DomainIdSpecified && info.RelationshipRoleSpecified)
                {
                    if (info.DomainId == domainId && info.RelationshipType == relationshipType)
                    {
                        return info.RelationshipRole;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Get the domain tage of circle's hidden repersentative.
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        private string GetHiddenRepresentativeDomainTag(ContactType contact)
        {
            if (contact.contactInfo == null)
                return string.Empty;

            if (contact.contactInfo.contactType != MessengerContactType.Circle)
                return string.Empty;

            return GetDomainTagFromNetworkInfo(contact.contactInfo.NetworkInfoList, DomainIds.WLDomain);
        }

        private string GetDomainTagFromNetworkInfo(NetworkInfoType[] infoList, int domainId)
        {
            if (infoList == null)
                return string.Empty;

            foreach (NetworkInfoType info in infoList)
            {
                if (info.DomainIdSpecified && !string.IsNullOrEmpty(info.DomainTag))
                {
                    if (info.DomainId == domainId)
                        return info.DomainTag;
                }
            }

            return string.Empty;
        }

        private bool AddToContactTable(long CID, ContactType contact)
        {
            if (contact.contactInfo == null)
                return false;

            lock (contactTable)
                contactTable[CID] = contact;
            return true;
        }

        private bool AddHiddenRepresentative(string domainTag, ContactType contact)
        {
            if (string.IsNullOrEmpty(domainTag) || contact == null)
            {
                return false;
            }

            if (contact.contactInfo == null)
            {
                return false;
            }

            if (contact.fDeleted == true)
            {
                lock (HiddenRepresentatives)
                    HiddenRepresentatives.Remove(domainTag.ToLowerInvariant());
                return false;
            }

            lock (HiddenRepresentatives)
                HiddenRepresentatives[domainTag.ToLowerInvariant()] = contact;

            return true;
        }

        /// <summary>
        /// Set MyProperties to default value.
        /// </summary>
        public void InitializeMyProperties()
        {
            if (!MyProperties.ContainsKey(AnnotationNames.MSN_IM_MBEA))
                MyProperties[AnnotationNames.MSN_IM_MBEA] = "0";

            if (!MyProperties.ContainsKey(AnnotationNames.MSN_IM_GTC))
                MyProperties[AnnotationNames.MSN_IM_GTC] = "1";

            if (!MyProperties.ContainsKey(AnnotationNames.MSN_IM_BLP))
                MyProperties[AnnotationNames.MSN_IM_BLP] = "0";

            if (!MyProperties.ContainsKey(AnnotationNames.MSN_IM_MPOP))
                MyProperties[AnnotationNames.MSN_IM_MPOP] = "0";

            if (!MyProperties.ContainsKey(AnnotationNames.MSN_IM_RoamLiveProperties))
                MyProperties[AnnotationNames.MSN_IM_RoamLiveProperties] = "1";

            if (!MyProperties.ContainsKey(AnnotationNames.Live_Profile_Expression_LastChanged))
                MyProperties[AnnotationNames.Live_Profile_Expression_LastChanged] = XmlConvert.ToString(DateTime.MinValue, "yyyy-MM-ddTHH:mm:ss.FFFFFFFzzzzzz");
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Save the <see cref="XMLContactList"/> into a specified file.
        /// </summary>
        /// <param name="filename"></param>
        public override void Save(string filename)
        {
            Version = Properties.Resources.XMLContactListVersion;
            base.Save(filename);
        }
        #endregion

    }
};
