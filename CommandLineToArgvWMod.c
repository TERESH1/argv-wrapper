#pragma once
#include <Windows.h>

/*************************************************************************
 * CommandLineToArgvW            [SHCORE.@]
 *
 * We must interpret the quotes in the command line to rebuild the argv
 * array correctly:
 * - arguments are separated by spaces or tabs
 * - quotes serve as optional argument delimiters
 *   '"a b"'   -> 'a b'
 * - escaped quotes must be converted back to '"'
 *   '\"'      -> '"'
 * - consecutive backslashes preceding a quote see their number halved with
 *   the remainder escaping the quote:
 *   2n   backslashes + quote -> n backslashes + quote as an argument delimiter
 *   2n+1 backslashes + quote -> n backslashes + literal quote
 * - backslashes that are not followed by a quote are copied literally:
 *   'a\b'     -> 'a\b'
 *   'a\\b'    -> 'a\\b'
 * - in quoted strings, consecutive quotes see their number divided by three
 *   with the remainder modulo 3 deciding whether to close the string or not.
 *   Note that the opening quote must be counted in the consecutive quotes,
 *   that's the (1+) below:
 *   (1+) 3n   quotes -> n quotes
 *   (1+) 3n+1 quotes -> n quotes plus closes the quoted string
 *   (1+) 3n+2 quotes -> n+1 quotes plus closes the quoted string
 * - in unquoted strings, the first quote opens the quoted string and the
 *   remaining consecutive quotes follow the above rule.
 */

__declspec(dllexport) WCHAR** WINAPI CommandLineToArgvWMod(const WCHAR* cmdline, int* numargs) {
   int qcount, bcount;
   const WCHAR * s;
   WCHAR ** argv;
   DWORD argc;
   WCHAR * d;

   if (!numargs) {
      SetLastError(ERROR_INVALID_PARAMETER);
      return NULL;
   }

   if ( * cmdline == 0) {
      /* Return the path to the executable */
      DWORD len, deslen = MAX_PATH, size;

      size = sizeof(WCHAR * ) * 2 + deslen * sizeof(WCHAR);
      for (;;) {
         if (!(argv = LocalAlloc(LMEM_FIXED, size))) return NULL;
         len = GetModuleFileNameW(0, (WCHAR * )(argv + 2), deslen);
         if (!len) {
            LocalFree(argv);
            return NULL;
         }
         if (len < deslen) break;
         deslen *= 2;
         size = sizeof(WCHAR * ) * 2 + deslen * sizeof(WCHAR);
         LocalFree(argv);
      }
      argv[0] = (WCHAR * )(argv + 2);
      argv[1] = NULL;
      * numargs = 1;

      return argv;
   }

   /* --- First count the arguments */
   argc = 1;
   s = cmdline;
   /* The first argument, the executable path, follows special rules */
   if ( * s == '"') {
      /* The executable path ends at the next quote, no matter what */
      s++;
      while ( * s)
         if ( * s++ == '"')
            break;
   } else {
      /* The executable path ends at the next space, no matter what */
      while ( * s && * s != ' ' && * s != '\t')
         s++;
   }
   /* skip to the first argument, if any */
   while ( * s == ' ' || * s == '\t')
      s++;
   if ( * s)
      argc++;

   /* Analyze the remaining arguments */
   qcount = bcount = 0;
   while ( * s) {
      if (( * s == ' ' || * s == '\t') && qcount == 0) {
         /* skip to the next argument and count it if any */
         while ( * s == ' ' || * s == '\t')
            s++;
         if ( * s)
            argc++;
         bcount = 0;
      } else if ( * s == '\\') {
         /* '\', count them */
         bcount++;
         s++;
      } else if ( * s == '"') {
         /* '"' */
         if ((bcount & 1) == 0)
            qcount++; /* unescaped '"' */
         s++;
         bcount = 0;
         /* consecutive quotes, see comment in copying code below */
         while ( * s == '"') {
            qcount++;
            s++;
         }
         qcount = qcount % 3;
         if (qcount == 2)
            qcount = 0;
      } else {
         /* a regular character */
         bcount = 0;
         s++;
      }
   }

   /* Allocate in a single lump, the string array, and the strings that go
    * with it. This way the caller can make a single LocalFree() call to free
    * both, as per MSDN.
    */
   argv = LocalAlloc(LMEM_FIXED, (argc + 1) * sizeof(WCHAR * ) + (lstrlenW(cmdline) + 1) * sizeof(WCHAR));
   if (!argv)
      return NULL;

   /* --- Then split and copy the arguments */
   argv[0] = d = lstrcpyW((WCHAR * )(argv + argc + 1), cmdline);
   argc = 1;
   /* The first argument, the executable path, follows special rules */
   if ( * d == '"') {
      /* The executable path ends at the next quote, no matter what */
      s = d + 1;
      while ( * s) {
         if ( * s == '"') {
            s++;
            break;
         }
         * d++ = * s++;
      }
   } else {
      /* The executable path ends at the next space, no matter what */
      while ( * d && * d != ' ' && * d != '\t')
         d++;
      s = d;
      if ( * s)
         s++;
   }
   /* close the executable path */
   * d++ = 0;
   /* skip to the first argument and initialize it if any */
   while ( * s == ' ' || * s == '\t')
      s++;
   if (! * s) {
      /* There are no parameters so we are all done */
      argv[argc] = NULL;
      * numargs = argc;
      return argv;
   }

   /* Split and copy the remaining arguments */
   argv[argc++] = d;
   qcount = bcount = 0;
   while ( * s) {
      if (( * s == ' ' || * s == '\t') && qcount == 0) {
         /* close the argument */
         * d++ = 0;

         /* skip to the next one and initialize it if any */
         do {
            s++;
         } while ( * s == ' ' || * s == '\t');
         if ( * s)
            argv[argc++] = d;
      } else if ( * s == '\\') {
         * d++ = * s++;
      } else if ( * s == '"') {
         * d++ = * s++;
         /* Now count the number of consecutive quotes. Note that qcount
          * already takes into account the opening quote if any, as well as
          * the quote that lead us here.
          */
         while ( * s == '"') {
            * d++ = * s++;
         }
      } else {
         /* a regular character */
         * d++ = * s++;
      }
   }
   * d = '\0';
   argv[argc] = NULL;
   * numargs = argc;

   return argv;
}