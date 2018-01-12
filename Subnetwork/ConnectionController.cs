﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomSocket;
using System.Net;

namespace Subnetwork
{
    class ConnectionController
    {
        public const char PARAM_SEPARATOR = ' ';
        public const int ADDRESS_POSITION = 0;
        public const int MASK_POSITION = 1;
        public const int FIRST_ADDRESS_POS = 0;
        public const int FIRST_CAPACITY_POS = 1;
        public const int SECOND_ADDRESS_POS = 2;
        public const int SECOND_CAPACITY_POS = 3;

        private List<CSocket> sockets;
        private string NetworkAddress, ParentNetworkAddress; //prawdziwe adresy IP
        private string SubnetworkAddress, SubnetworkMask;    //adres tego subnetworka
        public List<SubnetworkAddress> ContainedSubnetworksAddresses { get; set; }
        private Dictionary<SubnetworkAddress, List<Tuple<IPAddress, IPAddress>>> OtherDomainSNPPAddressTranslation;
        private Dictionary<string[], List<SNP>> existingConnections;

        private List<Link> linkList;

        public ConnectionController()
        {
            NetworkAddress = Config.getProperty("NetworkAddress");
            ParentNetworkAddress = Config.getProperty("ParentNetworkAddress");

            SubnetworkAddress = Config.getProperty("SubnetworkAddress");
            SubnetworkMask = Config.getProperty("SubnetworkMask");

            OtherDomainSNPPAddressTranslation = new Dictionary<Subnetwork.SubnetworkAddress, List<Tuple<IPAddress, IPAddress>>>();
            existingConnections = new Dictionary<string[], List<SNP>>();
            ContainedSubnetworksAddresses = new List<SubnetworkAddress>();
            linkList = new List<Link>();
            LoadContainedSubnetworks();
        }

        public void AddKeyToDictionary(SubnetworkAddress key)
        {
            if (!OtherDomainSNPPAddressTranslation.ContainsKey(key))
                OtherDomainSNPPAddressTranslation.Add(key, new List<Tuple<IPAddress, IPAddress>>());
        }

        public void AddValueToDictionary(SubnetworkAddress key, Tuple<IPAddress, IPAddress> value)
        {
            List<Tuple<IPAddress, IPAddress>> list = GetFromDictionary(key);
            list.Add(value);
        }

        public List<Tuple<IPAddress, IPAddress>> GetFromDictionary(SubnetworkAddress address)
        {
            foreach (KeyValuePair<SubnetworkAddress, List<Tuple<IPAddress, IPAddress>>> entry in OtherDomainSNPPAddressTranslation)
            {
                if (entry.Key.subnetAddress.Equals(address.subnetAddress) && entry.Key.subnetMask.Equals(address.subnetMask))
                    return entry.Value;
            }
            return null;
        }

        public void LoadContainedSubnetworks()
        {
            LogClass.Log("loading contained subnetworks.");
            string fileName = Config.getProperty("ContainedSubnetworks");
            string[] loadedFile = LoadFile(fileName);
            string[] subnetworkParams = null;
            foreach (string str in loadedFile)
            {
                subnetworkParams = str.Split(PARAM_SEPARATOR);
                ContainedSubnetworksAddresses.Add(new SubnetworkAddress(subnetworkParams[ADDRESS_POSITION], subnetworkParams[MASK_POSITION]));
                Console.WriteLine(str);
            }
        }

        private string[] LoadFile(String fileName)
        {
            string[] fileLines = System.IO.File.ReadAllLines(fileName);
            return fileLines;
        }

        private List<SNPP> RouteTableQuery(string pathBegin, string pathEnd, int capacity)
        {
            List<SNPP> SNPPlist = SubnetworkServer.callRouteTableQueryInRC(pathBegin, pathEnd, capacity);

            return SNPPlist;
        }

        private Tuple<SNP, SNP> LinkConnectionRequest(SNPP connectionBegin, SNPP connectionEnd, int capacity)
        {
            Tuple<SNP, SNP> SNPpair = SubnetworkServer.callLinkConnectionRequestInLRM(connectionBegin, connectionEnd, capacity);
            return SNPpair;
        }

        private bool DeletePeerCoordinationOut(SNP lastSNPinThisDomain, string pathEndAddressFromDifferentDomain)
        {
            throw new NotImplementedException();
        }

        private void DeleteLinkConnectionRequest(SNP SNPpathBegin, SNP SNPpathEnd)
        {
            SubnetworkServer.CallDeleteLinkConnectionRequestInLRM(SNPpathBegin, SNPpathEnd, 0);
        }


        // % % % % % % % % % % % % % % % % % % % % % % % % % // 
        // %%%%%%%%%%%%%%%%% GŁOWNA METODA %%%%%%%%%%%%%%%%% //    
        // % % % % % % % % % % % % % % % % % % % % % % % % % //

        public bool ConnectionRequestFromNCC(string pathBegin, string pathEnd, int capacity)
        {
            string PathEndAddressFromDifferentDomain = null;

            //Lista SNP dla tworzonego aktualnie polaczenia
            List<SNP> SNPList = new List<SNP>();

            //zakladamy, ze adres poczatkowy zawsze jest w naszej domenie, jezeli dostalismy requesta od NCC
            //dodaj SNP z labelem 0 dla poczatku sciezki
            SNPList.Add(new SNP(0, pathBegin, capacity, pathBegin, pathEnd));

            //sprawdzamy, czy adres koncowy jest w naszej domenie
            if (!IPAddressExtensions.IsInSameSubnet(IPAddress.Parse(pathEnd), IPAddress.Parse(SubnetworkAddress), IPAddress.Parse(SubnetworkMask)))
            {
                PathEndAddressFromDifferentDomain = pathEnd;

                //sprawdza, czy adres docelowy jest w innej podsieci i podmienia
                foreach (SubnetworkAddress domainAddress in OtherDomainSNPPAddressTranslation.Keys)
                {

                    if (IPAddressExtensions.IsInSameSubnet(IPAddress.Parse(pathEnd), domainAddress.subnetAddress, domainAddress.subnetMask))
                    {
                        Random random = new Random();
                        List<Tuple<IPAddress, IPAddress>> translationsList = OtherDomainSNPPAddressTranslation[domainAddress];
                        Tuple<IPAddress, IPAddress> foundTranslation = translationsList[random.Next(translationsList.Count)];
                        IPAddress translatedAddress = foundTranslation.Item1;
                        pathEnd = translatedAddress.ToString();
                    }
                }
            }

            //dodaj SNP z labelem 0 dla konca sciezki
            SNPList.Add(new SNP(0, pathEnd, capacity, pathBegin, pathEnd));
            LogClass.Log("RouteTableQuery called between: " + pathBegin + " and: " + pathEnd);
            List<SNPP> SNPPList = RouteTableQuery(pathBegin, pathEnd, capacity);

            for (int index = 0; index < SNPPList.Count; index += 2)
            {
                SNPP SNPPpathBegin = SNPPList[index];
                SNPP SNPPpathEnd = SNPPList[index + 1];
                Tuple<SNP, SNP> SNPpair = LinkConnectionRequest(SNPPpathBegin, SNPPpathEnd, capacity);
                SNPList.Add(SNPpair.Item1);
                SNPList.Add(SNPpair.Item2);
            }

            //Zapamietaj SNPlist z polaczeniem mdzy takimi adresami
            existingConnections.Add(new string[] { pathBegin, pathEnd }, SNPList);

            //Wysłanie ConnectionRequesta do podsieci, jeżeli na liscie SNP zajdą się 2 adresy brzegowe tej podsieci

            for (int index = 0; index < SNPList.Count - 1; index++)
            {
                SNP SNPpathBegin = SNPList[index];
                for (int jndex = index + 1; jndex < SNPList.Count; jndex++)
                {
                    SNP SNPpathEnd = SNPList[jndex];

                    if (BelongsToSubnetwork(SNPpathBegin, SNPpathEnd))
                    {
                        if (ConnectionRequestOut(SNPpathBegin, SNPpathEnd))
                        {
                            LogClass.Log("Subnetwork Connection set properly.");
                        }
                        else
                        {
                            LogClass.Log("Epic fail.");
                            return false;
                        }
                    }
                }

            }

            //Wyslanie PeerCoordination jezeli zestawiane polaczenie przebiega przez 2 domeny

            if (PathEndAddressFromDifferentDomain != null)
            {
                //TODO: sprawdz, czy ktorys z SNP ma adres SNPP brzegowego tej domeny
                SNP lastSNPinThisDomain = null;
                foreach (SNP snp in SNPList)
                {
                    foreach (List<Tuple<IPAddress, IPAddress>> list in OtherDomainSNPPAddressTranslation.Values)
                    {
                        foreach (Tuple<IPAddress, IPAddress> tuple in list)
                        {
                            if (tuple.Item1.ToString() == snp.Address)
                            {
                                lastSNPinThisDomain = snp;
                            }
                        }
                    }
                }


                if (PeerCoordinationOut(lastSNPinThisDomain, PathEndAddressFromDifferentDomain))
                {
                    LogClass.Log("PeerCoordination OK.");
                }
                else
                {
                    LogClass.Log("PeerCoordination FAIL.");
                };
            }

            return true;  //Jesli polaczenie zestawiono poprawnie
        }

        public bool DeleteConnection(string pathBegin, string pathEnd)
        {
            List<SNP> SNPList = existingConnections[new string[] { pathBegin, pathEnd }];

            //w kazdym SNP ustaw "deleting" na true
            SNPList.ForEach(x => x.Deleting = true);

            string PathEndAddressFromDifferentDomain = null;

            //usuniecie alokacji w LRM
            for (int index = 0; index < SNPList.Count; index += 2)
            {
                SNP SNPpathBegin = SNPList[index];
                SNP SNPpathEnd = SNPList[index + 1];
                DeleteLinkConnectionRequest(SNPpathBegin, SNPpathEnd);
            }

            //Wysłanie DeleteConnectionRequesta do podsieci, jeżeli na liscie SNP znajdą się 2 adresy brzegowe tej podsieci

            for (int index = 0; index < SNPList.Count - 1; index++)
            {
                SNP SNPpathBegin = SNPList[index];
                for (int jndex = index + 1; jndex < SNPList.Count; jndex++)
                {
                    SNP SNPpathEnd = SNPList[jndex];
                    if (BelongsToSubnetwork(SNPpathBegin, SNPpathEnd))
                    {
                        if (DeleteConnectionRequestOut(SNPpathBegin, SNPpathEnd))
                        {
                            LogClass.Log("Deleting " + SNPpathBegin.Address + " - " + SNPpathEnd + " successful.");
                        }
                        else
                        {
                            LogClass.Log("Epic fail xD");
                            return false;
                        }
                    }
                }
            }

            //sprawdzamy, czy adres koncowy jest w naszej domenie
            if (!IPAddressExtensions.IsInSameSubnet(IPAddress.Parse(pathEnd), IPAddress.Parse(SubnetworkAddress), IPAddress.Parse(SubnetworkMask)))
            {
                PathEndAddressFromDifferentDomain = pathEnd;

                //TODO: sprawdz, czy ktorys z SNP ma adres SNPP brzegowego tej domeny
                SNP lastSNPinThisDomain = null;
                foreach (SNP snp in SNPList)
                {
                    foreach (List<Tuple<IPAddress, IPAddress>> list in OtherDomainSNPPAddressTranslation.Values)
                    {
                        foreach (Tuple<IPAddress, IPAddress> tuple in list)
                        {
                            if (tuple.Item1.ToString() == snp.Address)
                            {
                                lastSNPinThisDomain = snp;
                            }
                        }
                    }
                }

                if (DeletePeerCoordinationOut(lastSNPinThisDomain, PathEndAddressFromDifferentDomain))
                {
                    LogClass.Log("DeletePeerCoordination OK.");
                }
                else
                {
                    LogClass.Log("PeerCoordination FAIL.");
                };
            }
            return true;  //Jesli polaczenie zestawiono poprawnie
        }

        public bool DeleteConnectionRequestOut(SNP pathBegin, SNP pathEnd)
        {
            //wysyla do cc poziom niżej wiadomosc usuwaj jak szalony konik
            IPAddress subnetworkAddress = null;
            IPAddress subnetworkAddressMask = null;

            foreach (SubnetworkAddress sub in ContainedSubnetworksAddresses)
            {
                if (IPAddressExtensions.IsInSameSubnet(sub.subnetAddress, IPAddress.Parse(pathBegin.Address), sub.subnetMask))
                {
                    subnetworkAddress = sub.subnetAddress;
                    subnetworkAddressMask = sub.subnetMask;
                }
            }
            SubnetworkAddress subnetAddress = new SubnetworkAddress(subnetworkAddress.ToString(), subnetworkAddressMask.ToString());
            SubnetworkServer.SendDeleteConnectionRequest(pathBegin.Address, pathEnd.Address, subnetAddress);
            return true;
        }

        public bool ConnectionRequestFromCC(SNP pathBegin, SNP pathEnd)
        {
            List<SNPP> SNPPList = RouteTableQuery(pathBegin.Address, pathEnd.Address, pathBegin.OccupiedCapacity);

            //Lista SNP dla tworzonego aktualnie polaczenia
            List<SNP> SNPList = new List<SNP>();

            SNPList.Add(pathBegin);
            SNPList.Add(pathEnd);

            for (int index = 0; index < SNPPList.Count; index += 2)
            {
                SNPP SNPPpathBegin = SNPPList[index];
                SNPP SNPPpathEnd = SNPPList[index + 1];
                Tuple<SNP, SNP> SNPpair = LinkConnectionRequest(SNPPpathBegin, SNPPpathEnd, pathBegin.OccupiedCapacity);
                SNPList.Add(SNPpair.Item1);
                SNPList.Add(SNPpair.Item2);
            }

            //Zapamietaj SNPlist z polaczeniem mdzy takimi adresami
            existingConnections.Add(new string[] { pathBegin.Address, pathEnd.Address }, SNPList);

            //Wysłanie ConnectionRequesta do podsieci, jeżeli na liscie SNP zajdą się 2 adresy brzegowe tej podsieci

            for (int index = 0; index < SNPList.Count - 1; index++)
            {
                SNP SNPpathBegin = SNPList[index];
                for (int jndex = index + 1; jndex < SNPList.Count; jndex++)
                {
                    SNP SNPpathEnd = SNPList[jndex];

                    if (BelongsToSubnetwork(SNPpathBegin, SNPpathEnd))
                    {
                        if (ConnectionRequestOut(SNPpathBegin, SNPpathEnd))
                        {
                            LogClass.Log("Subnetwork Connection set properly.");
                        }
                        else
                        {
                            LogClass.Log("Epic fail.");
                            return false;
                        }
                    }
                }
            }

            return true;  //Jesli polaczenie zestawiono poprawnie
        }

        private bool BelongsToSubnetwork(SNP SNPstart, SNP SNPend)
        {
            //sprawdza, czy ma taka pare na liscie 
            foreach (SubnetworkAddress subAddress in ContainedSubnetworksAddresses)
            {
                if (IPAddressExtensions.IsInSameSubnet(IPAddress.Parse(SNPstart.Address), IPAddress.Parse(SNPend.Address), subAddress.subnetMask))
                    return true;
            }
            return false;
        }

        private bool ConnectionRequestOut(SNP pathBegin, SNP pathEnd)
        {
            //wysyla do cc poziom niżej wiadomosc connection request
            IPAddress subnetworkAddress = null;
            IPAddress subnetworkAddressMask = null;

            foreach (SubnetworkAddress sub in ContainedSubnetworksAddresses)
            {
                if (IPAddressExtensions.IsInSameSubnet(sub.subnetAddress, IPAddress.Parse(pathBegin.Address), sub.subnetMask))
                {
                    subnetworkAddress = sub.subnetAddress;
                    subnetworkAddressMask = sub.subnetMask;
                }
            }
            SubnetworkAddress subnetAddress = new SubnetworkAddress(subnetworkAddress.ToString(), subnetworkAddressMask.ToString());
            SubnetworkServer.SendConnectionRequest(pathBegin, pathEnd, subnetAddress);
            return true;
        }

        public bool PeerCoordinationIn(SNP pathBegin, string pathEnd)
        {

            //Lista SNP dla tworzonego aktualnie polaczenia
            List<SNP> SNPList = new List<SNP>();

            //sprawdza, z ktorej domeny przyszedl SNP i podmienia jego adres na adres swojego SNPP brzegowego
            foreach (SubnetworkAddress domainAddress in OtherDomainSNPPAddressTranslation.Keys)
            {
                if (IPAddressExtensions.IsInSameSubnet(IPAddress.Parse(pathBegin.Address), domainAddress.subnetAddress, domainAddress.subnetMask))
                {
                    Tuple<IPAddress, IPAddress> foundTranslation = OtherDomainSNPPAddressTranslation[domainAddress].Find(x => x.Item1.ToString() == pathBegin.Address);
                    IPAddress translatedAddress = foundTranslation.Item2;
                    pathBegin.Address = translatedAddress.ToString();
                }
            }

            //przepustowosc bierzemy z przekazanego SNP

            SNPList.Add(new SNP(pathBegin.Label, pathBegin.Address, pathBegin.OccupiedCapacity, pathBegin.PathBegin, pathBegin.PathEnd));
            SNPList.Add(new SNP(0, pathEnd, pathBegin.OccupiedCapacity, pathBegin.PathBegin, pathBegin.PathEnd));

            List<SNPP> SNPPList = RouteTableQuery(pathBegin.Address, pathEnd, pathBegin.OccupiedCapacity);

            for (int index = 0; index < SNPPList.Count; index += 2)
            {
                SNPP SNPPpathBegin = SNPPList[index];
                SNPP SNPPpathEnd = SNPPList[index + 1];
                Tuple<SNP, SNP> SNPpair = LinkConnectionRequest(SNPPpathBegin, SNPPpathEnd, pathBegin.OccupiedCapacity);
                SNPList.Add(SNPpair.Item1);
                SNPList.Add(SNPpair.Item2);
            }

            //Wysłanie ConnectionRequesta do podsieci, jeżeli na liscie SNP zajdą się 2 adresy brzegowe tej podsieci

            for (int index = 0; index < SNPList.Count - 1; index++)
            {
                SNP SNPpathBegin = SNPList[index];
                for (int jndex = index + 1; jndex < SNPList.Count; jndex++)
                {
                    SNP SNPpathEnd = SNPList[jndex];

                    if (BelongsToSubnetwork(SNPpathBegin, SNPpathEnd))
                    {
                        if (ConnectionRequestOut(SNPpathBegin, SNPpathEnd))
                        {
                            LogClass.Log("Subnetwork Connection set properly.");
                        }
                        else
                        {
                            LogClass.Log("Epic fail.");
                            return false;
                        }
                    }
                }
            }

            return true;  //Jesli polaczenie zestawiono poprawnie
        }

        private bool PeerCoordinationOut(SNP SNPpathBegin, string AddressPathEnd)
        {
            SubnetworkServer.SendPeerCoordination(SNPpathBegin, AddressPathEnd);
            return true;
        }

    }
}
