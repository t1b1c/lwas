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

using LWAS.Extensible.Interfaces.Routing;

namespace LWAS.Infrastructure.Routing
{
    public class RoutingManager : IRoutingManager
    {
        IRoutesCollection _settingsRoutes;
        public IRoutesCollection SettingsRoutes
        {
            get { return _settingsRoutes; }
        }

        IRoutesCollection _runtimeSettingsRoutes;
        public IRoutesCollection RuntimeSettingsRoutes
        {
            get { return _runtimeSettingsRoutes; }
        }

        IRoutesCollection _screensRoutes;
        public IRoutesCollection ScreensRoutes
        {
            get { return _screensRoutes; }
        }

        IRoutesCollection _applicationsRoutes;
        public IRoutesCollection ApplicationsRoutes
        {
            get { return _applicationsRoutes; }
        }

        IRoutingAgentsCollection _agents;
        public IRoutingAgentsCollection Agents
        {
            get { return _agents; }
        }

        public IRoutingAgent Add
        {
            set { _agents.Add(value); }
        }

        public RoutingManager()
        {
            _settingsRoutes = new RoutesCollection();
            _runtimeSettingsRoutes = new RoutesCollection();
            _screensRoutes = new RoutesCollection();
            _applicationsRoutes = new RoutesCollection();
            _agents = new RoutingAgentsCollection();
        }

        public void Load()
        {
            _settingsRoutes = new RoutesCollection();
            _runtimeSettingsRoutes = new RoutesCollection();
            _screensRoutes = new RoutesCollection();
            _applicationsRoutes = new RoutesCollection();

            foreach (IRoutingAgent agent in _agents)
                agent.Load(this);

            foreach (IRoute route in _settingsRoutes)
                route.Resolve();
            foreach (IRoute route in _runtimeSettingsRoutes)
                route.Resolve();
            foreach (IRoute route in _screensRoutes)
                route.Resolve();
            foreach (IRoute route in _applicationsRoutes)
                route.Resolve();
        }
    }
}
