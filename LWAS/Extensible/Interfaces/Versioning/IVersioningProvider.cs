/*
 * Copyright 2006-2013 TIBIC SOLUTIONS
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;

namespace LWAS.Extensible.Interfaces.Versioning
{
    public interface IVersioningProvider
    {
        string RootPath { get; }

        void Refresh(string path);
        void Refresh(string path, string file);
        void Ignore(string path, string what);
        void Add(string path, string file);
        void Remove(string path, string file);
        void Lock(string path, string file);
        string LockInfo(string path, string file);
        void Unlock(string path, string file);
        void Restore(string path, string file);
        void Restore(string path, string file, string version);
        void Commit(string path, string comment);
        void Commit(string path, string file, string comment);

        IEnumerable<string> List(string path);
        IEnumerable<string> Revisions(string path, string file);

        bool IsVersioned(string path);
        bool IsVersioned(string path, string file);
    }
}
