using Newtonsoft.Json;
using Organilog.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Organilog.IServices
{
  
    class ChangeAddressForSociete : BaseService
    {
        IServiceProvider serviceProvider;
        public ChangeAddressForSociete(IServiceProvider service)
        {
            //
            this.serviceProvider = service;

        }

    }
}
