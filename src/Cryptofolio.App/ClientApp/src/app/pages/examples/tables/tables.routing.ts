import { Routes } from "@angular/router";

import { RegularComponent } from "./regular/regular.component";
import { ExtendedComponent } from "./extended/extended.component";
import { NgxDatatablesComponent } from "./ngxdatatables/ngxdatatables.component";

export const TablesRoutes: Routes = [
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
        path: "ngx-datatable",
        component: NgxDatatablesComponent
      }
    ]
  }
];
