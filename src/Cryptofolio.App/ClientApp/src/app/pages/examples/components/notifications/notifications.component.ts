import { Component } from "@angular/core";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: "app-notifications",
  templateUrl: "notifications.component.html"
})
export class NotificationsComponent {
  constructor(public toastr: ToastrService) {}
  showNotification(from, align) {
    const color = Math.floor(Math.random() * 5 + 1);

    switch (color) {
      case 1:
        this.toastr.info(
          '<span class=" tim-icons icon-bell-55"></span> Welcome to <b>Black Dashboard PRO Angular</b> - a beautiful premium admin for every web developer.',
          "",
          {
            timeOut: 8000,
            closeButton: true,
            enableHtml: true,
            toastClass: "alert alert-info alert-with-icon",
            positionClass: "toast-" + from + "-" + align
          }
        );
        break;
      case 2:
        this.toastr.success(
          '<span class=" tim-icons icon-bell-55"></span> Welcome to <b>Black Dashboard PRO Angular</b> - a beautiful notification for every web developer.',
          "",
          {
            timeOut: 8000,
            closeButton: true,
            enableHtml: true,
            toastClass: "alert alert-success alert-with-icon",
            positionClass: "toast-" + from + "-" + align
          }
        );
        break;
      case 3:
        this.toastr.warning(
          '<span class=" tim-icons icon-bell-55"></span> Welcome to <b>Black Dashboard PRO Angular</b> - a beautiful notification for every web developer.',
          "",
          {
            timeOut: 8000,
            closeButton: true,
            enableHtml: true,
            toastClass: "alert alert-warning alert-with-icon",
            positionClass: "toast-" + from + "-" + align
          }
        );
        break;
      case 4:
        this.toastr.error(
          '<span class=" tim-icons icon-bell-55"></span> Welcome to <b>Black Dashboard PRO Angular</b> - a beautiful notification for every web developer.',
          "",
          {
            timeOut: 8000,
            enableHtml: true,
            closeButton: true,
            toastClass: "alert alert-danger alert-with-icon",
            positionClass: "toast-" + from + "-" + align
          }
        );
        break;
      case 5:
        this.toastr.show(
          '<span class=" tim-icons icon-bell-55"></span> Welcome to <b>Black Dashboard PRO Angular</b> - a beautiful notification for every web developer.',
          "",
          {
            timeOut: 8000,
            closeButton: true,
            enableHtml: true,
            toastClass: "alert alert-primary alert-with-icon",
            positionClass: "toast-" + from + "-" + align
          }
        );
        break;
      default:
        break;
    }
  }
}
