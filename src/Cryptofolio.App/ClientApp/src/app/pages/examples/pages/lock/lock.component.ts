import { Component, OnInit, OnDestroy } from "@angular/core";

@Component({
  selector: "app-lock",
  templateUrl: "lock.component.html"
})
export class LockComponent implements OnInit, OnDestroy {
  focus;
  constructor() {}

  ngOnInit() {
    var body = document.getElementsByTagName("body")[0];
    body.classList.add("lock-page");
  }
  ngOnDestroy() {
    var body = document.getElementsByTagName("body")[0];
    body.classList.remove("lock-page");
  }
}
