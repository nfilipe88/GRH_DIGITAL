import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MinhasAusencias } from './minhas-ausencias';

describe('MinhasAusencias', () => {
  let component: MinhasAusencias;
  let fixture: ComponentFixture<MinhasAusencias>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MinhasAusencias]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MinhasAusencias);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
