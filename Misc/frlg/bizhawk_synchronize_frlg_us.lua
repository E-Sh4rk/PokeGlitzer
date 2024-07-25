local storagePtr = 0x5010;
local storageOffset = 4;
local storageSize = 80*14*30 + 9*14;

local teamAddr = 0x24284;
local teamSize = 100*6;

-- Init mmf
lastUpc = 0;
comm.mmfWriteBytes("bizhawk_up_c", {lastUpc});
comm.mmfCopyFromMemory("bizhawk_up", 0, storageSize, "EWRAM");
lastDownc = 0;
comm.mmfWriteBytes("bizhawk_down_c", {lastDownc});
comm.mmfCopyFromMemory("bizhawk_down", 0, storageSize, "EWRAM");

lastUpc2 = 0;
comm.mmfWriteBytes("bizhawk2_up_c", {lastUpc2});
comm.mmfCopyFromMemory("bizhawk2_up", 0, teamSize, "EWRAM");
lastDownc2 = 0;
comm.mmfWriteBytes("bizhawk2_down_c", {lastDownc2});
comm.mmfCopyFromMemory("bizhawk2_down", 0, teamSize, "EWRAM");

while true do
	emu.frameadvance();
	local ptr = memory.read_u32_le(storagePtr, "IWRAM") - 0x02000000;
	if ptr > 0 and ptr + storageOffset + storageSize <= 0x3FFFF then
		-- Boxes
		local upc = comm.mmfReadBytes("bizhawk_up_c", 1)[0];
		if upc ~= lastUpc then
			lastUpc = upc;
			comm.mmfCopyToMemory("bizhawk_up", ptr + storageOffset, storageSize, "EWRAM");
		end
		lastDownc = (lastDownc+1) % 256;
		comm.mmfCopyFromMemory("bizhawk_down", ptr + storageOffset, storageSize, "EWRAM");
		comm.mmfWriteBytes("bizhawk_down_c", {lastDownc});
		-- Party
		local upc2 = comm.mmfReadBytes("bizhawk2_up_c", 1)[0];
		if upc2 ~= lastUpc2 then
			lastUpc2 = upc2;
			comm.mmfCopyToMemory("bizhawk2_up", teamAddr, teamSize, "EWRAM");
		end
		lastDownc2 = (lastDownc2+1) % 256;
		comm.mmfCopyFromMemory("bizhawk2_down", teamAddr, teamSize, "EWRAM");
		comm.mmfWriteBytes("bizhawk2_down_c", {lastDownc2});
	end
end
