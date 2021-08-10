import { Routes } from "@angular/router";

import { RegularComponent } from "./regular/regular.component";
import { ExtendedComponent } from "./extended/extended.component";
import { ValidationComponent } from "./validation/validation.component";
import { WizardComponent } from "./wizard/wizard.component";

export const FormsRoutes: Routes = [
  {
    path: "",
    children: [
      {
        path: "regular",
        component: RegularComponent
      }
    ]
  },
  {
    path: "",
    children: [
      {
        path: "extended",
        component: ExtendedComponent
      }
    ]
  },
  {
    path: "",
    children: [
      {
        path: "validation",
        component: ValidationComponent
      }
    ]
  },
  {
    path: "",
    children: [
      {
        path: "wizard",
        component: WizardComponent
      }
    ]
  }
];
