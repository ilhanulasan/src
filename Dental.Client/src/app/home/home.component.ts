import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { ToothIconComponent } from '../shared/tooth-icon.component';

@Component({
  selector: 'app-home',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, TranslatePipe, ToothIconComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent {
  readonly heroImageUrl =
    'https://dentistry.emu.edu.tr/PublishingImages/Stock/dentistry-01.jpg?RenditionID=3';
}
