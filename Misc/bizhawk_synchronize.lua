local storagePtr = 0x5D94;
local storageOffset = 4;
local storageSize = 80*14*30;

lastUpc = 0;
comm.mmfWriteBytes("bizhawk_upc", {lastUpc});
comm.mmfCopyFromMemory("bizhawk_up", 0, storageSize, "EWRAM");
while true do
	emu.frameadvance();
	local ptr = memory.read_u32_le(storagePtr, "IWRAM") - 0x02000000;
	if ptr > 0 and ptr + storageOffset + storageSize <= 0x3FFFF then
		local upc = comm.mmfReadBytes("bizhawk_upc", 1)[0];
		if upc ~= lastUpc then
			lastUpc = upc;
			comm.mmfCopyToMemory("bizhawk_up", ptr + storageOffset, storageSize, "EWRAM");
		end
		comm.mmfCopyFromMemory("bizhawk_down", ptr + storageOffset, storageSize, "EWRAM");
	end
end
