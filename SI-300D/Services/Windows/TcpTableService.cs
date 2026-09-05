using SI_300D.Models;
using System.Net;
using System.Runtime.InteropServices;

namespace SI_300D.Services.Windows
{
    public class TcpTableService
    {
        private const int AF_INET = 2;
        private const int AF_INET6 = 23;

        private const int TCP_TABLE_OWNER_PID_ALL = 5;

        private const int NO_ERROR = 0;
        private const int ERROR_INSUFFICIENT_BUFFER = 122;

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern uint GetExtendedTcpTable(
            IntPtr tcpTable,
            ref int tcpTableLength,
            bool sort,
            int ipVersion,
            int tableClass,
            uint reserved);

        public List<TcpTableEntry> GetTcpTable()
        {
            var entries = new List<TcpTableEntry>();

            entries.AddRange(GetTcpTable(AF_INET));
            entries.AddRange(GetTcpTable(AF_INET6));

            return entries;
        }

        private List<TcpTableEntry> GetTcpTable(int ipVersion)
        {
            var entries = new List<TcpTableEntry>();

            int bufferSize = 0;

            var result = GetExtendedTcpTable(
                IntPtr.Zero,
                ref bufferSize,
                true,
                ipVersion,
                TCP_TABLE_OWNER_PID_ALL,
                0);

            if (result != ERROR_INSUFFICIENT_BUFFER)
                return entries;

            IntPtr tcpTable = Marshal.AllocHGlobal(bufferSize);

            try
            {
                result = GetExtendedTcpTable(
                    tcpTable,
                    ref bufferSize,
                    true,
                    ipVersion,
                    TCP_TABLE_OWNER_PID_ALL,
                    0);

                if (result != NO_ERROR)
                    return entries;

                int entryCount = Marshal.ReadInt32(tcpTable);

                IntPtr rowPtr = IntPtr.Add(
                    tcpTable,
                    sizeof(int));

                int rowSize = ipVersion == AF_INET
                    ? Marshal.SizeOf<MibTcpRowOwnerPid>()
                    : Marshal.SizeOf<MibTcp6RowOwnerPid>();

                for (int i = 0; i < entryCount; i++)
                {
                    if (ipVersion == AF_INET)
                    {
                        var row = Marshal.PtrToStructure<MibTcpRowOwnerPid>(rowPtr);

                        entries.Add(new TcpTableEntry
                        {
                            LocalAddress = new IPAddress(row.LocalAddress).ToString(),
                            LocalPort = ConvertPort(row.LocalPort),
                            RemoteAddress = new IPAddress(row.RemoteAddress).ToString(),
                            RemotePort = ConvertPort(row.RemotePort),
                            State = row.State,
                            ProcessId = row.ProcessId
                        });
                    }
                    else
                    {
                        var row = Marshal.PtrToStructure<MibTcp6RowOwnerPid>(rowPtr);

                        var localAddress = new IPAddress(row.LocalAddress, row.LocalScopeId);
                        var remoteAddress = new IPAddress(row.RemoteAddress, row.RemoteScopeId);

                        entries.Add(new TcpTableEntry
                        {
                            LocalAddress = localAddress.ToString(),
                            LocalPort = ConvertPort(row.LocalPort),
                            RemoteAddress = remoteAddress.ToString(),
                            RemotePort = ConvertPort(row.RemotePort),
                            State = row.State,
                            ProcessId = row.ProcessId
                        });
                    }

                    rowPtr = IntPtr.Add(rowPtr, rowSize);
                }

                return entries;
            }
            finally
            {
                Marshal.FreeHGlobal(tcpTable);
            }
        }

        private static int ConvertPort(uint port)
        {
            return (int)((port >> 8) | ((port & 0xFF) << 8));
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MibTcpRowOwnerPid
        {
            public uint State;
            public uint LocalAddress;
            public uint LocalPort;
            public uint RemoteAddress;
            public uint RemotePort;
            public uint ProcessId;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MibTcp6RowOwnerPid
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] LocalAddress;

            public uint LocalScopeId;
            public uint LocalPort;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] RemoteAddress;

            public uint RemoteScopeId;
            public uint RemotePort;

            public uint State;
            public uint ProcessId;
        }
    }
}
