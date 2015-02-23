// $begin{copyright}
//
// This file is confidential and proprietary.
//
// Copyright (c) IntelliFactory, 2004-2010.
//
// All rights reserved.  Reproduction or use in whole or in part is
// prohibited without the written consent of the copyright holder.
//-----------------------------------------------------------------
// $end{copyright}
namespace WebSharper.Formlets.JQueryUI

open WebSharper
open System.Web.UI

module internal Resources =
    open IntelliFactory.Formlets.Base
    /// Default CSS skin.
    type internal SkinResource() =
        inherit Core.Resources.BaseResource("Formlet.css")
