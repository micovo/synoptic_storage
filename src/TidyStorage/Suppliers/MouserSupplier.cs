using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TidyStorage.Suppliers.Data;

namespace TidyStorage.Suppliers
{
    public class MouserSupplier : Supplier
    {
        public MouserSupplier(string part_number) : base(part_number)
        {

        }

        public override string Name { get { return "Mouse"; } }

        public override string GetLink()
        {
            throw new NotImplementedException();
        }

        public override SupplierPart DownloadPart()
        {
            throw new NotImplementedException();
        }

        /*
         * 
                /*
                 * POST /service/searchapi.asmx HTTP/1.1
Host: cz.mouser.com
Content-Type: text/xml; charset=utf-8
Content-Length: length
SOAPAction: "http://api.mouser.com/service/SearchByPartNumber"

<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Header>
    <MouserHeader xmlns="http://api.mouser.com/service">
      <AccountInfo>
        <PartnerID>string</PartnerID>
      </AccountInfo>
    </MouserHeader>
  </soap:Header>
  <soap:Body>
    <SearchByPartNumber xmlns="http://api.mouser.com/service">
      <mouserPartNumber>string</mouserPartNumber>
    </SearchByPartNumber>
  </soap:Body>
</soap:Envelope>*/



    }
}
