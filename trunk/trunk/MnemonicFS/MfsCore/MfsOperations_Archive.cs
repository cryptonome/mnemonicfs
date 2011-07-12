﻿/**
 * Copyright © 2009, Najeeb Shaikh
 * All rights reserved.
 * http://www.mnemonicfs.org
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 
 * - Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 * 
 * - Neither the name of the MnemonicFS Team, nor the names of its
 * contributors may be used to endorse or promote products
 * derived from this software without specific prior written
 * permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
 * COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
 * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 **/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MnemonicFS.MfsExceptions;
using System.IO;

namespace MnemonicFS.MfsCore {
    public partial class MfsOperations {
        [Serializable]
        public class _Archive : IDisposable {
            private MfsOperations _parent;
            private MfsDBOperations _dbOperations;

            private _Archive (MfsOperations parent) {
                _parent = parent;
                _dbOperations = new MfsDBOperations (_parent._userID, _parent._userSpecificPath);
            }

            private static _Archive _theObject = null;

            internal static _Archive GetObject (MfsOperations parent) {
                if (_theObject == null) {
                    _theObject = new _Archive (parent);
                }

                return _theObject;
            }

            #region << IDisposable Members >>

            public void Dispose () {
                _theObject = null;
            }

            #endregion << IDisposable Members >>

            #region << Archiving Operations >>

            public void New (GroupingType groupingType, ulong groupingID, string opDirPath, string opArchiveName, string password) {
                if (password != null && password.Equals (string.Empty)) {
                    throw new MfsIllegalArgumentException (
                        MfsErrorMessages.GetMessage (MessageType.NULL_OR_EMPTY, "Password")
                    );
                }

                List<ulong> filesInGrouping = null;

                switch (groupingType) {
                    case GroupingType.ASPECT:
                        _parent.AspectObj.DoAspectChecks (groupingID);
                        filesInGrouping = _dbOperations.GetDocumentsAppliedWithAspect (groupingID);
                        break;
                    case GroupingType.BRIEFCASE:
                        _parent.BriefcaseObj.DoBriefcaseChecks (groupingID);
                        filesInGrouping = _dbOperations.GetDocumentsInBriefcase (groupingID);
                        break;
                    case GroupingType.COLLECTION:
                        _parent.CollectionObj.DoCollectionChecks (groupingID);
                        filesInGrouping = _dbOperations.GetDocumentsInCollection (groupingID);
                        break;
                    case GroupingType.NONE:
                        throw new MfsIllegalArgumentException (
                            MfsErrorMessages.GetMessage (MessageType.BAD_ARG, "Grouping type")
                        );
                }

                List<byte[]> filesData = new List<byte[]> (filesInGrouping.Count);
                List<string> fileNames = new List<string> (filesInGrouping.Count);
                foreach (ulong fileID in filesInGrouping) {
                    byte[] fileData = _parent.FileObj.RetrieveOriginal (fileID);
                    filesData.Add (fileData);
                    string fileName = _parent.FileObj.GetName (fileID);
                    fileNames.Add (fileName);
                }

                MfsStorageDevice.ArchiveFiles (filesData, fileNames, opDirPath, opArchiveName, password);
            }

            #endregion << Archiving Operations >>
        }
    }
}