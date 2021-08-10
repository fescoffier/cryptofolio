import { Routes } from "@angular/router";

import { GoogleComponent } from "./google/google.component";
import { FullscreenComponent } from "./fullscreen/fullscreen.component";
import { VectorComponent } from "./vector/vector.component";

export const MapsRoutes: Routes = [
  {
    path: "",
    children: [
      {
        path: "full-screen",
        component: FullscreenComponent
      }
    ]
  },
  {
    path: "",
    children: [
      {
        path: "google",
        component: GoogleComponent
      }
    ]
  },
  {
    path: "",
    children: [
      {
        path: "vector",
        component: VectorComponent
      }
    ]
  }
];
