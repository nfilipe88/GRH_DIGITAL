import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GestaoAusencias } from './gestao-ausencias';

describe('GestaoAusencias', () => {
  let component: GestaoAusencias;
  let fixture: ComponentFixture<GestaoAusencias>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GestaoAusencias]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GestaoAusencias);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
