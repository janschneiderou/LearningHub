﻿/**
 * ****************************************************************************
 * Copyright (C) 2018 Das Deutsche Institut für Internationale Pädagogische Forschung (DIPF)
 * <p/>
 * This library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * <p/>
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * <p/>
 * You should have received a copy of the GNU Lesser General Public License
 * along with this library.  If not, see <http://www.gnu.org/licenses/>.
 * <p/>
 * Contributors: Jan Schneider
 * ****************************************************************************
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectorHub
{
    public class FeedbackObject
    {
        public System.TimeSpan frameStamp { get; set; }
        public string applicationName { get; set; }
        public string verb { get; set; }

        public FeedbackObject(System.DateTime start,  string feedbackValue, string applicationName)
        {
        
            this.frameStamp = System.DateTime.Now.Subtract(start);
            this.applicationName = applicationName;
            this.verb = feedbackValue;
        
        }
    }
}
