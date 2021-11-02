module coronavirus_dashboard_summary.Utils.Attrs

open Giraffe.ViewEngine

let inline _proptype (prop: string) = attr "proptype" prop
let inline _itemtype (item: string) = attr "itemtype" item
let inline _itemprop (prop: string) = attr "itemprop" prop
let _itemscope                      = attr "itemscope" ""
let inline _as (prop: string)       = attr "as" prop
let _crossorigin                    = attr "crossorigin" ""
let baseTag                         = voidTag "base"
