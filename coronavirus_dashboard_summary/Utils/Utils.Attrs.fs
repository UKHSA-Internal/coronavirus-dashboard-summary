module coronavirus_dashboard_summary.Utils.Attrs

open Giraffe.ViewEngine

let _proptype (prop: string) = attr "proptype" prop

let _itemtype (item: string) = attr "itemtype" item

let _itemprop (prop: string) = attr "itemprop" prop

let _itemscope = attr "itemscope" ""

let _as (prop: string) = attr "as" prop

let _crossorigin = attr "crossorigin" ""

let baseTag = voidTag "base"
