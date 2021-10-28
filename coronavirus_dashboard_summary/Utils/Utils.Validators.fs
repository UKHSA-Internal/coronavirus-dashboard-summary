module coronavirus_dashboard_summary.Utils.Validators

open System.Text.RegularExpressions

let inline private (|Regex|_|) pattern input =
    let found = Regex.Match(input, pattern)
    
    match found.Success with
    | true  -> Some(List.tail [ for g in found.Groups -> g.Value ])
    | false -> None

let inline ValidatePostcode (postcode: string): string =
    match postcode.ToUpper() with
    | Regex @"^\s*([A-Z]{1,2}\d{1,2}[A-Z]?\s?\d{1,2}[A-Z]{1,2})\s*$" [ validated ] ->
        validated.Replace(" ", "").ToUpper()
    | _ -> ""
