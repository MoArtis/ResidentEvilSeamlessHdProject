//This is a snippet to Convert the RE3's SLD files into workable mask textures (as PNG files).
//
//This is all the code I added to Gemini's RE2 Mod Tools:
//(https://github.com/Gemini-Loboto3/RE2-Mod-tools)
//This snippet must be added to roomslicer.cpp
//
//Most of the code is a C to C++ conversion of Patrice Mandin sld.c and sld_depack.c:
//(https://github.com/pmandin/reevengi-tools)

#include <string>
#include <iostream>
#include <filesystem>
namespace fs = std::experimental::filesystem;

typedef struct {
	unsigned long unknown;
	unsigned long length;
} sld_header_t;

void sld_depack(MEM_STREAM *src, u8 **dstBufPtr, int *dstLength)
{
	u32 numblocks, buflen = 65536;
	u8 start, *dst;
	int i, j, count, offset, dstIndex = 0;

	*dstBufPtr = NULL;
	*dstLength = 0;

	MemStreamRead(src, &numblocks, sizeof(u32));

	if (numblocks == 0) {
		return;
	}

	dst = (u8*)malloc(buflen);

	for (i = 0; i < numblocks; i++) {

		MemStreamRead(src, &start, sizeof(u8));
		if (start & 0x80) {
			count = start & 0x7f;

			if (dstIndex + count > buflen) {
				buflen += 65536;
				dst = (u8*)realloc(dst, buflen);
			}

			MemStreamRead(src, &dst[dstIndex], count);
			dstIndex += count;
		}
		else {
			u32 tmp = start << 8;

			MemStreamRead(src, &start, sizeof(u8));
			tmp |= start;

			offset = (tmp & 0x7ff) + 4;
			count = (tmp >> 11) + 2;

			if (dstIndex + count > buflen) {
				buflen += 65536;
				dst = (u8*)realloc(dst, buflen);
			}

			for (j = 0; j < count; j++) {
				dst[dstIndex + j] = dst[dstIndex - offset + j];
			}
			dstIndex += count;
		}
	}

	*dstBufPtr = (u8*)realloc(dst, dstIndex);
	*dstLength = dstIndex;
}

void ConvertSld(LPCSTR filename, LPCSTR outfolder, LPCSTR outname)
{
	CreateDirectory(outfolder, NULL);
	char name[MAX_PATH + 1];

	MEM_STREAM str;
	MEM_STREAM *src = &str;
	sld_header_t sld_hdr;
	u32 offset;
	int i;

	MemStreamOpen(src, filename);

	printf("Converting %s\n", filename);

	offset = 0;
	i = 0;
	while (MemStreamRead(src, &sld_hdr, sizeof(sld_header_t))) {

		int fileLen = (int)sld_hdr.length;

		if (fileLen) {
			u8 *dstBuffer;
			int dstBufLen;
			char filename_tim[512];

			printf("Depacking camera %d: ", i);

			sld_depack(src, &dstBuffer, &dstBufLen);

			if (dstBuffer && dstBufLen) {

				Image img;
				Tim tim;
				tim.LoadTim(dstBuffer);
				img.CreateFromTim(&tim, 0);
				sprintf(name, "%s//%s%02d%s", outfolder, outname, i, ".png");
				img.SavePng(name);

				free(dstBuffer);

				printf("Saved as %s%02d%s\n", outname, i, ".png");
			}
			else
			{
				printf("Error during the depacking process");
			}
		}
		else {
			printf("Camera %d has no mask texture\n", i);
			fileLen = 8;
		}

		/* Next file */
		offset += fileLen;
		MemStreamSeek(src, offset, SEEK_SET);
		i++;
	}

	MemStreamClose(src);
}

int main()
{
	char filePath[MAX_PATH + 1];
	char fileName[MAX_PATH + 1];
	std::string path = "./RE3/SLD";
	for (const auto & entry : fs::directory_iterator(path))
	{	
		sprintf(filePath, entry.path().string().c_str());

		std::string pathStr = entry.path().filename().string();
		pathStr.resize(pathStr.size() - 4);

		sprintf(fileName, pathStr.c_str());

		std::cout << entry.path().filename().string().c_str() << "\n";

		ConvertSld(filePath, ".\\RE3\\PNG", fileName);
	}
}