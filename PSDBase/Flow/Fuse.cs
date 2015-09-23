using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSD.Base.Flow
{
        public interface MintBase
        {
                public string ToMessage();
        }

        public class Fuse
        {
                public class FuseHost
                {
                        public string SKTHead { set; get; }
                        public int InType { set; get; }
                }

                public MintBase Mint { set; get; }
                public List<FuseHost> Host { private set; get; }

                public Fuse()
                {
                        Host = null; Mint = null;
                }

                public Fuse SetHost(IEnumerable<string> hostCodes)
                {
                        if (hostCodes != null)
                                Host = hostCodes.Select(p => new FuseHost()
                                {
                                        SKTHead = p.Substring(0, p.IndexOf(',')),
                                        InType = int.Parse(p.IndexOf(',') + 1)
                                });
                        else
                                Host = null;
                        return this;
                }
                public Fuse SetHost(string hostGroup)
                {
                        return hostGroup == null ? this : SetHost(hostGroup.Split('&'));
                }

                public string ToMessage()
                {
                        if (Host == null)
                                return Mint.ToMessage();
                        else
                                return string.Join("&", Host.Select(p => p.SKTHead + "," + p.InType)) + ":" + Mint.ToMessage();
                }
        }
}
