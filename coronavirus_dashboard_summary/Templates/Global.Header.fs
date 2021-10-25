module coronavirus_dashboard_summary.Templates.Header  

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Accessibility

let GovUKHeader =
    header [ _class "govuk-header"; attr "role" "banner"; _data "module" "govuk-header"] [
        a [ _class "govuk-skip-link"; _href "#main-content" ] [ encodedText "Skip to main content" ]
        div [ _class "govuk-header__container govuk-width-container" ] [
            div [ _class "govuk-header__logo" ] [
                a [ _href "/"; _class "govuk-header__link govuk-header__link--homepage" ] [
                    span [ _class "govuk-header__logotype" ] [
                        tag "svg" [ _ariaHidden "true"
                                    attr "focusable" "false"
                                    _class "govuk-header__logotype-crown govuk-!-margin-right-1"
                                    attr "xmlns" "http://www.w3.org/2000/svg"
                                    attr "viewBox" "0 0 132 97"
                                    _height "30"
                                    _width "36"] [
                            voidTag "path" [
                                attr "fill" "currentColor"
                                attr "fill-rule" "evenodd"
                                attr "d" (
                                    "M25 30.2c3.5 1.5 7.7-.2 9.1-3.7 1.5-3.6-.2-7.8-3.9-9.2-3.6-1.4-7.6.3-9.1 "
                                    + "3.9-1.4 3.5.3 7.5 3.9 9zM9 39.5c3.6 1.5 7.8-.2 9.2-3.7 1.5-3.6-.2-7.8-3.9-9.1-3.6-1.5-7.6.2-9.1 "
                                    + "3.8-1.4 3.5.3 7.5 3.8 9zM4.4 57.2c3.5 1.5 7.7-.2 9.1-3.8 1.5-3.6-.2-7.7-3.9-9.1-3.5-1.5-7.6.3-9.1 "
                                    + "3.8-1.4 3.5.3 7.6 3.9 9.1zm38.3-21.4c3.5 1.5 7.7-.2 9.1-3.8 1.5-3.6-.2-7.7-3.9-9.1-3.6-1.5-7.6.3-9.1 "
                                    + "3.8-1.3 3.6.4 7.7 3.9 9.1zm64.4-5.6c-3.6 1.5-7.8-.2-9.1-3.7-1.5-3.6.2-7.8 3.8-9.2 3.6-1.4 7.7.3 9.2 "
                                    + "3.9 1.3 3.5-.4 7.5-3.9 9zm15.9 9.3c-3.6 1.5-7.7-.2-9.1-3.7-1.5-3.6.2-7.8 3.7-9.1 3.6-1.5 7.7.2 9.2 "
                                    + "3.8 1.5 3.5-.3 7.5-3.8 9zm4.7 17.7c-3.6 1.5-7.8-.2-9.2-3.8-1.5-3.6.2-7.7 3.9-9.1 3.6-1.5 7.7.3 "
                                    + "9.2 3.8 1.3 3.5-.4 7.6-3.9 9.1zM89.3 35.8c-3.6 1.5-7.8-.2-9.2-3.8-1.4-3.6.2-7.7 3.9-9.1 "
                                    + "3.6-1.5 7.7.3 9.2 3.8 1.4 3.6-.3 7.7-3.9 9.1zM69.7 17.7l8.9 4.7V9.3l-8.9 2.8c-.2-.3-.5-.6-.9-.9L72.4 "
                                    + "0H59.6l3.5 11.2c-.3.3-.6.5-.9.9l-8.8-2.8v13.1l8.8-4.7c.3.3.6.7.9.9l-5 15.4v.1c-.2.8-.4 1.6-.4 2.4 0 "
                                    + "4.1 3.1 7.5 7 8.1h.2c.3 0 .7.1 1 .1.4 0 .7 0 1-.1h.2c4-.6 7.1-4.1 7.1-8.1 "
                                    + "0-.8-.1-1.7-.4-2.4V34l-5.1-15.4c.4-.2.7-.6 1-.9zM66 92.8c16.9 0 32.8 1.1 47.1 3.2 4-16.9 "
                                    + "8.9-26.7 14-33.5l-9.6-3.4c1 4.9 1.1 7.2 0 10.2-1.5-1.4-3-4.3-4.2-8.7L108.6 76c2.8-2 5-3.2 "
                                    + "7.5-3.3-4.4 9.4-10 11.9-13.6 11.2-4.3-.8-6.3-4.6-5.6-7.9 1-4.7 5.7-5.9 8-.5 4.3-8.7-3-11.4-7.6-8.8 7.1-7.2 "
                                    + "7.9-13.5 2.1-21.1-8 6.1-8.1 12.3-4.5 20.8-4.7-5.4-12.1-2.5-9.5 6.2 3.4-5.2 7.9-2 7.2 3.1-.6 "
                                    + "4.3-6.4 7.8-13.5 7.2-10.3-.9-10.9-8-11.2-13.8 2.5-.5 7.1 1.8 11 7.3L80.2 60c-4.1 4.4-8 5.3-12.3 "
                                    + "5.4 1.4-4.4 8-11.6 8-11.6H55.5s6.4 7.2 7.9 11.6c-4.2-.1-8-1-12.3-5.4l1.4 16.4c3.9-5.5 8.5-7.7 "
                                    + "10.9-7.3-.3 5.8-.9 12.8-11.1 13.8-7.2.6-12.9-2.9-13.5-7.2-.7-5 3.8-8.3 7.1-3.1 2.7-8.7-4.6-11.6-9.4-6.2 "
                                    + "3.7-8.5 3.6-14.7-4.6-20.8-5.8 7.6-5 13.9 2.2 21.1-4.7-2.6-11.9.1-7.7 8.8 2.3-5.5 7.1-4.2 8.1.5.7 3.3-1.3 "
                                    + "7.1-5.7 7.9-3.5.7-9-1.8-13.5-11.2 2.5.1 4.7 1.3 7.5 3.3l-4.7-15.4c-1.2 4.4-2.7 7.2-4.3 8.7-1.1-3-.9-5.3 "
                                    + "0-10.2l-9.5 3.4c5 6.9 9.9 16.7 14 33.5 14.8-2.1 30.8-3.2 47.7-3.2z"
                                )
                            ]
                            voidTag "image" [
                                _class "govuk-header__logotype-crown-fallback-image"
                                _height "32"
                                _width "36"
                                _src (
                                    "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACQAAAAgCAYAAAB6kdqOAAAABmJLR0QA"
                                    + "/wD/AP+gvaeTAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAB3RJTUUH4AgXEBkknAt7fAAAA0VJREFU"
                                    + "WMPtl02IjlEUx/93Rs0YFiJSmlkgSU2+ZmGFNGI2PhdvjLJkMdRYiclHyl7U2CiThZKFpiRSksUs"
                                    + "KM0GTVhggRnCTJhe08/m/4471/M+3jHjIzl1e57nfPzPueeee+5zpX+JgIp446EwCUEdkrTEnw9C"
                                    + "CCcnglc1CYn6LKnVY/BPLtdUP2fzjWbEst+doQLwUdIBScMeHeYVfkmxAtOBqjI6uyhPu8rYVAHT"
                                    + "84o/01kIQcA518QI0JwTf7ukokd7ziSbJY1IGgTOhRDGlaHaZMZ3zN8OXATagPnAVGBBpLfAvPnW"
                                    + "uQhst+2dBLP2R0FsMkC7v2PqApYnvA3WOxHxTpi3IdFdboxRsl67fW5Kg5mXALQCjUA30FmmZg6a"
                                    + "/yrivTLvYFZNAZ3GbLSPmObFAW1OhEfjogMEVAP9lg8DdclyxctWZx1sU52BdzSx25xm6aUFI8Cs"
                                    + "nJ03x88ArMgIaAUQEt2sEpllXwAvM88eYGbJ2Tg2wPMomKfjsAuxz9FYgBrgoQEvpFkBpgFNP+hX"
                                    + "BaCQ11+AJmOl/Av2/RCoEdCRpjwxOGv+op85yT2pRcY4m8jSJT82RdKXBONj8r3Hz0shhGUJYIuk"
                                    + "mdFfA5LehhCuJU32UoS1N8fXpxJwt3fDkYwZjukdyVI1ZhR1Y0ZtfocRyY7Yd3cKvtTNayPQEBlc"
                                    + "N9ZAArTOz7bIX1upMSa6A5Zfj3gN9tUFLB0zCfeNEi0EeiPDeqAI3APOAzvN/wBcAVYDbzxWm/fB"
                                    + "OjvssMcY9RFur32VqC5N3W4Lbvo5LTkW0rbfE7E+eZSoJ+P4GT1ejB372l1uRzzxTPBWvg/cjRpY"
                                    + "E3Daupc9y2rgKnDD773AZeuctk2p4d71KJhXBJ7kbc9tVqwH9gHro9ST6B+I3vuBd1myKFM7/L4W"
                                    + "2G8f2Gdu3ygCfXFHBbbY+HiG/uFoOToy5Mct2xKfAEAfUKykkZ0ywGOgBdgKDEVOXwBnPPoyaqQv"
                                    + "kr+I+EPGajE2wKlKAlrF76OVFd3LmOhtr9JLYcZ/7JQyurckrZH0WNIzSa8lPZL0XtJb38We51w0"
                                    + "kVQvqUbSXEl1khZLmiOpQdJCSbd/+no80Svyr8D8T38NfQWcfES5EXNkCgAAAABJRU5ErkJggg=="
                                )
                            ]
                        ]
                        span [ _class "govuk-header__logotype-text" ] [ encodedText "GOV.UK" ]
                    ]
                    span [ _class "govuk-header__product-name" ] [ encodedText "Coronavirus (COVID-19) in the UK" ]
                ]
                button [ _class "mobile-menu-button govuk-button"; _id "mobile-menu-btn"] [
                    encodedText "Menu ▼"
                ]
            ]
        ]
    ]
