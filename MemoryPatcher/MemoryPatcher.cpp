#include "stdafx.h"
#include <tlhelp32.h>
#include "resource.h"

#define PATCH_SIZE			24
#define KEY_CODE			("\x47\x65\x74\x53\x79\x73\x74\x65\x6D\x57\x6F\x77\x36\x34\x44\x69\x72\x65\x63\x74\x6F\x72\x79\x41")
#define NEW_CODE			("\x53\x65\x74\x53\x79\x73\x74\x65\x6D\x57\x6F\x77\x36\x34\x44\x69\x72\x65\x63\x74\x6F\x72\x79\x41")

typedef HANDLE(__stdcall * OPENTHREAD)(DWORD,BOOL,DWORD);

//////////////////////////////////////////////////////////////////////////

int PATCH_BASE_ADDRESS = 0x00400000;
int PATCH_MAX_ADDRESS = 0x0100000;

VOID SuspendResumeProcess(DWORD dwProcessID,BOOL bSuspend)
{
    BOOL bExist;
    HANDLE hThread=NULL;
    HANDLE hSnapshot=NULL;
    THREADENTRY32 ThreadEntry32;

    OPENTHREAD OpenThread = (OPENTHREAD) GetProcAddress(GetModuleHandleA("kernel32"),"OpenThread");

    hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD,dwProcessID);

    if(hSnapshot != INVALID_HANDLE_VALUE) {
        ThreadEntry32.dwSize = sizeof(THREADENTRY32);
        bExist = Thread32First(hSnapshot,&ThreadEntry32);
        while(bExist){
            if(ThreadEntry32.th32OwnerProcessID == dwProcessID) {
                hThread= OpenThread(THREAD_SUSPEND_RESUME,FALSE,ThreadEntry32.th32ThreadID);
                if(hThread != NULL){
                    if(bSuspend)
                        SuspendThread(hThread);
                    else
                        ResumeThread(hThread);

                    CloseHandle(hThread);
                }
            }
            bExist = Thread32Next(hSnapshot,&ThreadEntry32);
        }
        CloseHandle(hSnapshot);
    }
}

BOOL CrackIt(DWORD dwProcessID)
{
    SetLastError(0);

    BOOL bContinueRun=TRUE;
    BOOL bPatchSucess =FALSE;
    DWORD dwOldProtection,dwDummy;

    HANDLE hProcess =OpenProcess(PROCESS_ALL_ACCESS, FALSE, dwProcessID);

    BYTE OldKeyCode[PATCH_SIZE];

    SuspendResumeProcess(dwProcessID,FALSE);
    Sleep(1000);
    SuspendResumeProcess(dwProcessID,TRUE);

    int offset = 0;
    while(offset < 0x0100000 && bContinueRun)
    {
        int calcOffset=PATCH_BASE_ADDRESS + offset;

        ReadProcessMemory(hProcess, (LPVOID)calcOffset, OldKeyCode, PATCH_SIZE,&dwDummy);
        if( !memcmp(OldKeyCode,(BYTE *)KEY_CODE, PATCH_SIZE) ){
            VirtualProtectEx(hProcess, (LPVOID)calcOffset, PATCH_SIZE,PAGE_EXECUTE_READWRITE, &dwOldProtection);

            WriteProcessMemory(hProcess, (LPVOID)calcOffset, NEW_CODE, PATCH_SIZE,&dwDummy);

            VirtualProtectEx(hProcess, (LPVOID)calcOffset, PATCH_SIZE,dwOldProtection, &dwDummy);

            bPatchSucess=TRUE;
            bContinueRun=false;
        }
        else{
            offset++;
        }
    }

    //for debug
    if(!bPatchSucess)
        MessageBoxA(NULL,"Cannot Match Code In Program !\n","ERROR",0);

    SuspendResumeProcess(dwProcessID,FALSE);

    return bPatchSucess;
}

int WINAPI WinMain (HINSTANCE hInstance, HINSTANCE hPrevInstance,
                    PSTR szCmdLine, int iCmdShow)
{
	int parms[3];
	int i=0;

	char *p = strstr(szCmdLine," ");
	while(p)
	{
		p[0]='\0';
		parms[i++]=atoi(szCmdLine);
		szCmdLine = p + 1;
		/* Get next token: */
		p = strstr( szCmdLine, " ");
	}
		parms[i]=atoi(szCmdLine);

		PATCH_BASE_ADDRESS=parms[1];
		PATCH_MAX_ADDRESS=parms[2];

		CrackIt(parms[0]);

    return 0;
}