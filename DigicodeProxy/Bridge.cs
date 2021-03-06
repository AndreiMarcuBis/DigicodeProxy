﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace DigicodeProxy
{
    class Bridge
    {
        Socket local_end;
        Socket remote_end;

        public Bridge(Socket local_end, Socket remote_end)
        {
            this.local_end = local_end;
            this.remote_end = remote_end;
        }

        public Socket get_local_end()
        {
            return local_end;
        }

        public Socket get_remote_end()
        {
            return remote_end;
        }

        public void transfer_to_local()
        {
            byte[] b = new byte[2048];

            try
            {
                int size = remote_end.Receive(b);

                if (size > 0)
                    local_end.Send(b.Take(size).ToArray());
                else
                    close();
            }
            catch (SocketException)
            {
                close();
            }
            catch (System.Security.SecurityException)
            {
                close();
            }
        }

        public void transfer_to_remote()
        {
            byte[] b = new byte[2048];

            try
            {
                int size = local_end.Receive(b);

                if (size > 0)
                    remote_end.Send(b.Take(size).ToArray());
                else
                    close();
            }
            catch (SocketException)
            {
                close();
            }
            catch (System.Security.SecurityException)
            {
                close();
            }
        }

        public bool is_closed()
        {
            return local_end == null && remote_end == null;
        }

        public bool use_socket(Socket s)
        {
            if (local_end == s)
            {
                transfer_to_remote();
                return true;
            }
            else if (remote_end == s)
            {
                transfer_to_local();
                return true;
            }

            return false;
        }

        private void close()
        {
            local_end.Close();
            remote_end.Close();

            local_end = null;
            remote_end = null;
        }
    }
}
